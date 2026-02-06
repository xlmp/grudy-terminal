using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace Grudy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        RunningCMD Runn;
        dbConfig DBConfig;
        Dictionary<string, string> Macros;
        public MainWindow()
        {
            InitializeComponent();
            RszMe.SetWindow = this;

            tab.Items.Clear();
            Runn = new RunningCMD();

            DBConfig = new dbConfig();
            DBConfig.Init().GetAwaiter();
            
            tab.MouseDown += Tab_MouseDown;

            Loaded += (a, b) =>
            {
                Macros = DBConfig.Macros().GetAwaiter().GetResult();
                LoadTerminais().GetAwaiter();
                foreach(var mm in Macros)
                {
                    var mn = new MenuItem() { Header = mm.Key };
                    MacroScipts.Items.Add(mn);
                }
            };

            //MacroScipts.Items.Add(new MenuItem() { Header = "Criar Macro" });
            Macros = new Dictionary<string, string>();

            var cfg = DBConfig.SysConfig().GetAwaiter().GetResult();
            if(cfg != null)
            {
                if (cfg.WindowPosX != null) this.Left = cfg.WindowPosX.Value;
                if (cfg.WindowPosY != null) this.Top = cfg.WindowPosY.Value;
                if (cfg.WindowWidth != null) this.Width = cfg.WindowWidth.Value;
                if (cfg.WindowHeight != null) this.Height = cfg.WindowHeight.Value;
            }
            this.Closing += (a, b) => {
                DBConfig.SysConfigSave(new TConfigs() 
                {
                    WindowPosX = this.Left,
                    WindowPosY = this.Top,
                    WindowHeight = this.Height,
                    WindowWidth = this.Width
                }).GetAwaiter();
            };
        }

        private void Tab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
            }
            
        }

        terminal_1 CreatTerminal(TabItem e)
        {
            var t = new terminal_1();
            t.PushCommand += Terminals_PushCommand;
            t.OutCommand += Terminals_OutCommand;
            t.CurrentTabItem = e;
            t.TerminalName = (e.Header as TabItemHeader).Text;
            return t;
        }

        async Task LoadTerminais()
        {
            var terminais = await DBConfig.Terminals();

            if(terminais.Count == 0)
            {
                await NovoTerminal();
                return;
            }

            foreach (var term in terminais)
            {
                var tati = NewTabItem(term) ;
                var TT = CreatTerminal(tati);

                TT.DbTerminal = term;
                TT.CurrentDir = term.CurrentDir;
                tati.Content = TT;
                TabItemAdd(tati);

            }
            tab.SelectedIndex = 0;
        }
        TabItem NewTabItem(TTerminal? tt)
        {
            var ti = new TabItem()
            {
                Background = Color.FromArgb(50, 0, 0, 0).ToFrozenBrush(),
                Foreground = Color.FromRgb(30, 30, 30).ToFrozenBrush(),
                Header = new TabItemHeader() { Text = (tt == null || tt.Name == null ? "Novo Terminal" : tt.Name) },
            };
            (ti.Header as TabItemHeader).TabItemMe = ti;
            (ti.Header as TabItemHeader).CloseMe += (a, e) =>
            {
                CurrentTerminal(e).CallPushCommand("exit");
            };
            return ti;
        }
        void TabItemAdd(TabItem item)
        {
            if (tab.Items.Count > 1)
                tab.Items.RemoveAt(tab.Items.Count - 1);

            tab.Items.Add(item);
            tab.SelectedIndex = tab.Items.Count - 1;

            tab.Items.Add(new TabItem() { Header = "+", Tag = "ADD" });
        }
        async Task NovoTerminal()
        {           
                       
            var tati = NewTabItem(null);
            var TT = CreatTerminal(tati);

            var dbTT = await DBConfig.NewTerminalC(TT.Name, TT.CurrentDir);
            TT.DbTerminal = dbTT;

            tati.Content = TT;
            TabItemAdd(tati);

        }

        terminal_1? CurrentTerminal(TabItem? e = null)
        {
            if(e != null && e.Content != null)
            {
                return e.Content as terminal_1;
            }
            if (tab.SelectedIndex == -1) return null;
            return (tab.Items[tab.SelectedIndex] as TabItem).Content as terminal_1;
        }

        private void Terminals_OutCommand(object? sender, Tuple<terminal_1.OutCommandList, string> e)
        {
            var term = (sender as terminal_1);
            switch (e.Item1)
            {
                case terminal_1.OutCommandList.NEW_TERMINAL:
                    NovoTerminal().GetAwaiter();
                    break;

                case terminal_1.OutCommandList.CLOSE_ME:
                    DBConfig.RemoveTerminal(term.DbTerminal.Id).GetAwaiter();
                    tab.Items.Remove(term.CurrentTabItem);
                    if (tab.Items.Count > 0)
                        tab.SelectedIndex = tab.Items.Count - 1;

                    if (tab.Items.Count == 0)
                        CloseMeAll();
                    break;

                case terminal_1.OutCommandList.WINDOW_MINIMIZE:
                    WindowState = WindowState.Minimized;
                    break;

                case terminal_1.OutCommandList.CHANGED_CURRENT_DIR:
                    term.DbTerminal.CurrentDir = term.CurrentDir;
                    term.DbTerminal.Name = term.TerminalName;
                    DBConfig.UpdateTerminal(term.DbTerminal).GetAwaiter();
                    break;

                case terminal_1.OutCommandList.CHANGED_TERMINAL_NAME:
                    (term.CurrentTabItem.Header as TabItemHeader).Text = term.TerminalName;
                    term.DbTerminal.Name = term.TerminalName;
                    DBConfig.UpdateTerminal(term.DbTerminal).GetAwaiter();
                    break;

                case terminal_1.OutCommandList.CLOSE_ALL:
                    CloseMeAll();
                    break;

                case terminal_1.OutCommandList.MACRO_ADD:
                    {
                        int p = e.Item2.IndexOf("=");
                        if (p > 0)
                        {
                            string m = e.Item2.Substring(0, p).Trim();
                            string v = e.Item2.Substring(p + 1).Trim();

                            AddMacro(m, v);
                        }
                        else
                        {
                            AddMacro(null, e.Item2);
                        }

                    }
                    break;

                case terminal_1.OutCommandList.MACRO_LIST:
                    foreach(var m in Macros)
                    {
                        term.PrintLn($"  {m.Key} = {m.Value}");
                    }
                    break;

                case terminal_1.OutCommandList.INTERRUPT:
                    if (term.CurrentProcess != null)
                    {
                        term.CurrentProcess.Stop();
                        term.CurrentProcess = null;
                    }
                    break;
            }
                    
        }
        void AddMacro(string? m, string value)
        {
            //$"@MACRO{Macros.Count + 1}"
            if(m == null)
            {
                int C = Macros.Count + 1;
                string testename = $"@macro{C}";
                RE_TESTE:;
                if (Macros.ContainsKey(testename))
                {
                    C--;
                    testename = $"@macro{C}";
                    goto RE_TESTE;
                }
                m = testename;
            }
            Macros.Add(m, value);
            var mn = new MenuItem() { Header = m };
            mn.Click += (j, i) =>
            {
                string mm = (j as MenuItem).Header.ToString();
                if (Macros.ContainsKey(mm))
                {
                    string mv = Macros[mm];
                    CurrentTerminal().CallPushCommand(mv, true);
                }
            };
            MacroScipts.Items.Add(mn);
            DBConfig.MacroAdd(m, value).GetAwaiter();
        }


        void CloseMeAll()
        {
            var r = MessageBox.Show("Fechar todos os terminais do Grudy", "Tem Certeza disso!?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
                Close();
        }

        private void Terminals_PushCommand(object? sender, string e)
        {
            var term = (sender as terminal_1);
            if (term == null) return;

            string outR = "Not result";
            var spc = e.IndexOf(" ");

            if(e.Length > 1 && e.Substring(0,1) == "#")
            {
                string m = e.Substring(1);
                if (!Macros.ContainsKey(m))
                {
                    term.PrintLn("Macro não existe");
                    return;
                }
                e = Macros[m].Trim();
                term.CallPushCommand(e);
                return;
            }

            if(term.CurrentProcess != null)
            {
                term.CurrentProcess.Stop();
                term.CurrentProcess = null;
            }
            
            if (spc >= 0)
            {
                var c = e.Substring(0, spc);
                var p = e.Substring(spc + 1);
                term.CurrentProcess = new ShellProcess(c, p, term.CurrentDir);
            }
            else
            {
                term.CurrentProcess = new ShellProcess(e, "", term.CurrentDir);
            }

            term.CurrentProcess.OutPut += (a, _out_) =>
            {
                term.PrintLn(_out_ ?? "");
            };

            term.Wait(true);
            term.CurrentProcess.Exit += (a, e) => {
                term.Wait(false);
            };

            term.CurrentProcess.Start();

            //if (spc >= 0)
            //{
            //    var c = e.Substring(0, spc);
            //    var p = e.Substring(spc + 1);

            //    outR = Runn.StartCmd(c, p, term.CurrentDir);
            //}
            //else
            //{
            //    var Runn = new ShellProcess(e, "", term.CurrentDir);
            //    Runn.OutPut += (a, _out_) => {
            //        term.PrintLn(_out_ ?? "");
            //        //Application.Current.Dispatcher.InvokeAsync(() => {
            //        //    term.PrintLn(_out_ ?? "");
            //        //});                   
            //    };
            //    Runn.Start();
            //    //outR = Runn.StartCmd(e, term.CurrentDir);
            //}

            if (outR.Length > 0 && !term.IsWaint)
            {
                term.PrintLn(outR);
            }
        }
    }

}