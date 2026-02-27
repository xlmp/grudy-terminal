using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace Grudy
{
    /// <summary>
    /// Lógica interna para TMessageBox.xaml
    /// </summary>
    public partial class TMessageBox : Window
    {

        private MessageBoxResult _result = MessageBoxResult.None;
        private bool _hasCancel = false;
        private bool _hasNo = false;

        private TMessageBox(string message, string title,
                                 MessageBoxButton buttons,
                                 MessageBoxImage icon,
                                 MessageBoxResult defaultResult)
        {
            InitializeComponent();

            Title = string.IsNullOrWhiteSpace(title) ? "Mensagem" : title;
            MessageText.Text = message ?? string.Empty;

            // Ícone
            var src = GetIconSource(icon);
            if (src != null)
            {
                IconImage.Source = src;
                IconImage.Visibility = Visibility.Visible;
            }

            // Botões
            BuildButtons(buttons, defaultResult);

            // Foco inicial no botão padrão
            var defaultBtn = ButtonsPanel.Children.OfType<Button>()
                .FirstOrDefault(b => b.IsDefault);
            (defaultBtn ?? ButtonsPanel.Children.OfType<Button>().FirstOrDefault())?.Focus();

            // A janela é modal por padrão (usaremos ShowDialog no wrapper)
        }

        #region API Pública (Drop-in replacement)

        public static MessageBoxResult Show(string messageBoxText)
            => ShowInternal(null, messageBoxText, string.Empty,
                            MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None);

        public static MessageBoxResult Show(string messageBoxText, string caption)
            => ShowInternal(null, messageBoxText, caption,
                            MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None);

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
            => ShowInternal(null, messageBoxText, caption,
                            button, MessageBoxImage.None, MessageBoxResult.None);

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
            => ShowInternal(null, messageBoxText, caption,
                            button, icon, MessageBoxResult.None);

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
                                            MessageBoxButton button, MessageBoxImage icon)
            => ShowInternal(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption,
                                            MessageBoxButton button, MessageBoxImage icon,
                                            MessageBoxResult defaultResult)
            => ShowInternal(owner, messageBoxText, caption, button, icon, defaultResult);

        private static MessageBoxResult ShowInternal(Window? owner, string text, string caption,
                                                     MessageBoxButton buttons, MessageBoxImage icon,
                                                     MessageBoxResult defaultResult)
        {
            var dlg = new TMessageBox(text, caption, buttons, icon, defaultResult);
            if (owner != null)
            {
                dlg.Owner = owner;
            }

            dlg.ShowDialog();
            return dlg._result;
        }

        #endregion

        #region Construção dos botões

        private void BuildButtons(MessageBoxButton buttons, MessageBoxResult defaultResult)
        {
            ButtonsPanel.Children.Clear();

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK, isDefault: defaultResult is MessageBoxResult.OK or MessageBoxResult.None, isCancel: true);
                    _hasCancel = true; // Esc pode fechar como OK nesse caso
                    break;

                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, isDefault: defaultResult is MessageBoxResult.OK or MessageBoxResult.None);
                    AddButton("Cancelar", MessageBoxResult.Cancel, isDefault: defaultResult == MessageBoxResult.Cancel, isCancel: true);
                    _hasCancel = true;
                    break;

                case MessageBoxButton.YesNo:
                    AddButton("Sim", MessageBoxResult.Yes, isDefault: defaultResult is MessageBoxResult.Yes or MessageBoxResult.None);
                    AddButton("Não", MessageBoxResult.No, isDefault: defaultResult == MessageBoxResult.No);
                    _hasNo = true;
                    break;

                case MessageBoxButton.YesNoCancel:
                    AddButton("Sim", MessageBoxResult.Yes, isDefault: defaultResult is MessageBoxResult.Yes or MessageBoxResult.None);
                    AddButton("Não", MessageBoxResult.No, isDefault: defaultResult == MessageBoxResult.No);
                    AddButton("Cancelar", MessageBoxResult.Cancel, isDefault: defaultResult == MessageBoxResult.Cancel, isCancel: true);
                    _hasCancel = true;
                    _hasNo = true;
                    break;
            }
        }

        private void AddButton(string text, MessageBoxResult result, bool isDefault = false, bool isCancel = false)
        {
            var btn = new Button
            {
                Content = text,
                MinWidth = 90,
                Margin = new Thickness(8, 0, 0, 0),
                IsDefault = isDefault,
                IsCancel = isCancel,
                Padding = new Thickness(10, 4, 10, 4),
                BorderBrush = null,
            };

            btn.Click += (_, __) =>
            {
                _result = result;
                Close();
            };

            ButtonsPanel.Children.Add(btn);
        }

        #endregion

        #region Ícones

        private static ImageSource? GetIconSource(MessageBoxImage icon)
        {
            Icon? sysIcon = icon switch
            {
                MessageBoxImage.Information => SystemIcons.Information,
                MessageBoxImage.Warning => SystemIcons.Warning,
                MessageBoxImage.Error => SystemIcons.Error,
                MessageBoxImage.Question => SystemIcons.Question,
                _ => null
            };

            if (sysIcon == null) return null;

            var hIcon = sysIcon.Handle;
            var src = Imaging.CreateBitmapSourceFromHIcon(
                hIcon, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Congelar para usar em múltiplas threads (boas práticas)
            src.Freeze();
            return src;
        }

        #endregion

        #region Teclas de atalho (Enter/Esc) e comportamento

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Esc → Cancel se existir; senão, No se existir; senão, fecha sem alterar result
                if (_hasCancel)
                {
                    _result = MessageBoxResult.Cancel;
                    Close();
                    e.Handled = true;
                }
                else if (_hasNo)
                {
                    _result = MessageBoxResult.No;
                    Close();
                    e.Handled = true;
                }
            }
            // Enter é resolvido por IsDefault do botão
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Se ainda não houver _result (normal), assegura que temos um default lógico
            if (_result == MessageBoxResult.None)
            {
                // nada aqui: o IsDefault do botão cuida do Enter;
                // se o usuário fechar pela borda (X), adotamos Cancel se existir
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Se usuário fechou no X e nenhum botão foi clicado, defina um resultado coerente
            if (_result == MessageBoxResult.None)
            {
                if (_hasCancel) _result = MessageBoxResult.Cancel;
                else if (_hasNo) _result = MessageBoxResult.No;
                else _result = MessageBoxResult.OK;
            }
        }

        #endregion
    }
}

