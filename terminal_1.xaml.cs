using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Grudy
{
    /// <summary>
    /// Interação lógica para terminal_1.xam
    /// </summary>
    public partial class terminal_1 : UserControl
    {
        public TTerminal DbTerminal = null;
        public ShellProcess? CurrentProcess = null;
        public TabItem CurrentTabItem { get; set; }
        public event EventHandler<string> PushCommand;
        public event EventHandler<Tuple<OutCommandList, string>> OutCommand;  
        
        string msgPrompt = "#> ";
        public string CurrentDir = "c:\\";
        int StartText = 0;

        List<string> HistoriCommands { get; set; } = null;
        int HistoriCommandsCurrent { get; set; } = 0;
        Boolean HistoriCommandsAction { get; set; } = false;
        public terminal_1()
        {
            InitializeComponent();


            CurrentDir = Environment.CurrentDirectory;// GetEnvironmentVariable("windir");

            HistoriCommands = new List<string>();

            term.KeyDown += Term_KeyDown;
            this.Loaded += Terminal_1_Loaded;

            this.HelpeLis();            

            
        }
        bool Initialize = false;
        private void Terminal_1_Loaded(object sender, RoutedEventArgs e)
        {
            if (Initialize)
                return;

            Initialize = true;
            term.IsReadOnly = true;
            term.Text = "";
            ViPrompt(false);
            term.IsReadOnly = false;
        }
        string TerminalName_;
        public string TerminalName
        {
            set => TerminalName_ = value;
            get => TerminalName_;
        }

        List<LocalCommands> localCommands = new List<LocalCommands>();

        private void Term_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
        private void term_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key[] keysT =  [Key.Up, Key.Down, Key.Left, Key.Right];

            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (CurrentProcess != null)
                    CurrentProcess.Stop();
                this.CallOutCommandList(OutCommandList.INTERRUPT);
            }

            if (this.isWaint)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back && term.SelectionStart <= this.StartText) 
            {
                e.Handled = true;
            }
            else if(HistoriCommands.Count > 0 && (e.Key == Key.Up || e.Key == Key.Down) && (term.SelectionStart == term.Text.Length || HistoriCommandsAction))
            {                                
                e.Handled = true;

                if(HistoriCommandsCurrent < 0) HistoriCommandsCurrent = HistoriCommands.Count - 1;
                if(HistoriCommandsCurrent >= HistoriCommands.Count) HistoriCommandsCurrent = 0;

                int slen = term.SelectionStart;
                if (!HistoriCommandsAction)
                {                    
                    term.Text += HistoriCommands[HistoriCommandsCurrent];
                    HistoriCommandsAction = true;
                    term.SelectionStart = slen;
                }
                else
                {
                    term.Text = term.Text.Substring(0, slen) + HistoriCommands[HistoriCommandsCurrent];
                    term.SelectionStart = slen;
                }
                if (e.Key == Key.Down) HistoriCommandsCurrent--;
                if (e.Key == Key.Up) HistoriCommandsCurrent++;

            }
            else if (term.SelectionStart < this.StartText && !keysT.Contains(e.Key))
            {
                term.SelectionStart = term.Text.Length;
            }
            else if (HistoriCommandsAction && e.Key == Key.Back)
            {
                e.Handled = true;
                term.SelectionStart = term.Text.Length;
                HistoriCommandsAction = false;
            }
            else if(e.Key == Key.Tab)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                this.CapturCommand();
                e.Handled = true;
                this.ViPrompt();
            }
        }        
        void ViPrompt(bool addBr  = true)
        {
            if (this.isWaint)
                return;

            if (!Directory.Exists(this.CurrentDir))
                this.CurrentDir = Environment.CurrentDirectory;
 
            string _p = msgPrompt;
            if (msgPrompt == "%DIR%")
            {
                string[] par = this.CurrentDir.Split('\\');
                _p = $"#..{par[par.Length - 1]}> ";
            }
            this.term.Text += $"{((addBr && this.term.Text.Length > 0) ? "\n" : "")}{_p}";
            this.StartText = term.Text.Length;
            term.SelectionStart = this.StartText;
            term.Focus();
        }
        void Print(string value)
        {
            this.term.Text += $"{value}";
            this.StartText = term.Text.Length;
            term.SelectionStart = this.StartText;
            term.Focus();
            
        }
        public void PrintLn(string value)
        {
            this.Print(value);
            Print("\n");
        }
        public Boolean IsWaint { get => this.isWaint; }
        Boolean isWaint { get; set; } = false;
        public void Wait(Boolean e)
        {
            this.isWaint = e;
            if(e == false)
            {
                this.ViPrompt();
            }
        }
        public bool CallPushCommand(string command, bool addPrompt = false)
        {
            this.CapturCommand(command);
            if(addPrompt) this.ViPrompt();
            return true;
        }
        void CapturCommand(string? outCommand = null)
        {
            try
            {                
                term.IsReadOnly = true;
                string cmd = outCommand ?? this.term.Text.Substring(this.StartText, term.Text.Length - this.StartText).Trim();                

                var chk = localCommands.FirstOrDefault(e => e.CheckCommand(cmd));

                Print("\n");

                HistoriCommandsAction = false;
                if (!HistoriCommands.Contains(cmd))
                {
                    HistoriCommands.Add(cmd);
                    HistoriCommandsCurrent = HistoriCommands.Count - 1;                    
                }                

                if (chk != null)
                {
                    chk.Method(chk);
                }
                else if (PushCommand != null)
                {
                    PushCommand(this, cmd);

                }
                else {
                    PrintLn("Comando não encontrado");
                }
                term.IsReadOnly = false;

            }
            catch (Exception ex)
            {
                //PushCommand(this, $"echo Push Erro: {ex.Message}");
                PrintLn($"echo Push Erro: {ex.Message}");
                term.IsReadOnly = false;
            }
        }

        void HelpeLis()
        {
            localCommands.Add(new LocalCommands("help", "Lista os comandos", "", () =>
            {
                string lista = String.Join("\n", localCommands.Select(e => $"{e.Command} - {e.Help}").ToList());
                this.PrintLn(lista);
            }));
            localCommands.Add(new LocalCommands("cls", "Limpar a tela", "", () => { this.term.Text = ""; }));
            localCommands.Add(new LocalCommands("time", "Mostra a hora", "", () => { this.PrintLn($"Hora Agora: {(DateTime.Now.ToString("HH:mm:ss"))}"); }));
            localCommands.Add(new LocalCommands("day", "Mostra a hora", "", () => { this.PrintLn($"Hoje: {(DateTime.Now.ToString("dd/MM/yyyy"))}"); }));
            localCommands.Add(new LocalCommands("new", "Novo Terminal", "", () => { this.CallOutCommandList(OutCommandList.NEW_TERMINAL); }));
            localCommands.Add(new LocalCommands("exit", "Fechar Terminal", "", () => { this.CallOutCommandList(OutCommandList.CLOSE_ME); }));
            localCommands.Add(new LocalCommands("close", "Fechar Terminal", "", () => { this.CallOutCommandList(OutCommandList.CLOSE_ALL); }));
            localCommands.Add(new LocalCommands("cd", "Mudar Diretório ex: cd (directory)", "", (cmd) => 
            { 
                if(String.IsNullOrEmpty(cmd.GetArguments))
                {
                    PrintLn($"Current Dir: {this.CurrentDir}");
                }
                else if (!Directory.Exists(cmd.GetArguments))
                {
                    PrintLn($"Diretório não encontrado :{cmd.GetArguments}");
                }
                else
                {
                    this.CurrentDir = cmd.GetArguments;
                    Environment.CurrentDirectory = this.CurrentDir;

                    this.CallOutCommandList(OutCommandList.CHANGED_CURRENT_DIR);
                }
                    
            }));
            localCommands.Add(new LocalCommands("#dir", "Mudar Cursos do Prompt para a pasta ", "", (cmd) => {               

                this.msgPrompt = "%DIR%";
            }));

            localCommands.Add(new LocalCommands("ls", "Listar Arquivos", "", ListarArquivos ));
            localCommands.Add(new LocalCommands("macro.add", "Criar Macro", "", (cmd) => { 
                this.CallOutCommandList(OutCommandList.MACRO_ADD, cmd.GetArguments); 
            }));
            localCommands.Add(new LocalCommands("macro.list", "Listar Macros", "", () => {
                PrintLn($"Listar Macros");
                this.CallOutCommandList(OutCommandList.MACRO_LIST);
            }));

            localCommands.Add(new LocalCommands("term.name", "Listar Macros", "", (cmd) => {
                TerminalName = cmd.GetArguments;
                this.CallOutCommandList(OutCommandList.CHANGED_TERMINAL_NAME, cmd.GetArguments);
            }));
        }

        void CallOutCommandList(OutCommandList e, string? argus = null)
        {
            if (OutCommand != null)
                OutCommand(this, new Tuple<OutCommandList, string>(e, argus));
        }

        void ListarArquivos(LocalCommands cmd) 
        {
            string args = String.IsNullOrEmpty(cmd.GetArguments) ? this.CurrentDir : cmd.GetArguments;

            string[] files = Directory.GetFiles(args);
            string[] dirs = Directory.GetDirectories(args);

            string r = "";
            foreach(var d in dirs)
            {
                r += $"/{Path.GetFileName(d)} \n";
            }
            foreach (var d in files)
            {
                r += $"{Path.GetFileName(d)} \n";
            }

            PrintLn(r);
        }
        class LocalCommands
        {
            public string Command { get; set; }
            public Action<LocalCommands> Method { get; set; }
            public string Help { get; set; }
            public string Manual { get; set; }
            public LocalCommands(string name, string help, string manual, Action<LocalCommands> method)
            {
                this.Command = name;
                Method = method;
                Help = help;
                Manual = manual;
            }
            public LocalCommands(string name, string help, string manual, Action method):
                this(name, help,  manual, (a) => { method(); })
            {                
            }
            private string Arguments { get; set; }
            public Boolean CheckCommand(string c)
            {
                if(c.Length < Command.Length)
                    return false;

                Arguments = "";
                if (c.Substring(0, Command.Length) == Command)
                {
                    var spc = c.IndexOf(" ");

                    if (spc >= 0)
                    {
                        var d = c.Substring(0, spc);
                        Arguments = c.Substring(spc + 1);
                    }

                    return true;
                }
                return false;
            }
            public string GetArguments { get=> Arguments; }
        }
        public enum OutCommandList
        {
            NONE = 0,
            NEW_TERMINAL = 1,
            CLOSE_ME = 2,
            CLOSE_ALL = 21,
            CHANGED_CURRENT_DIR = 3,
            CHANGED_TERMINAL_NAME = 31,
            WINDOW_MINIMIZE = 4,
            MACRO_ADD = 5,
            MACRO_CLEAR = 51,
            MACRO_LIST = 52,
            MACRO_REMOVE = 53,
            INTERRUPT = 100,
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(sender == btnClose)
                CallOutCommandList(OutCommandList.CLOSE_ALL);

            if (sender == btnMin)
                CallOutCommandList(OutCommandList.WINDOW_MINIMIZE);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CallOutCommandList(OutCommandList.NEW_TERMINAL);
        }
    }
}
