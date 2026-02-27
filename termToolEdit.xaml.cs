using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Grudy
{
    /// <summary>
    /// Interação lógica para termToolEdit.xam
    /// </summary>
    public partial class termToolEdit : UserControl
    {
        public event EventHandler Exit;
        public termToolEdit()
        {
            InitializeComponent();
            this.DataContext = new termToolEditViewModel(); // Set the data context
        }
        string FileName
        {
            set => (this.DataContext as termToolEditViewModel).FileName = value;
            get => (this.DataContext as termToolEditViewModel).FileName;
        }
        public void OpenEdit(string file, string CurrentDir)
        {
            this.FileName = null;
            editor.TextClear();
            if (String.IsNullOrEmpty(file))
                return;

            if (!File.Exists(file) && File.Exists($"{CurrentDir}\\{file}"))
            {
                file = $"{CurrentDir}\\{file}";
            }
            if (File.Exists(file))
            {
                using (FileStream fStream = new FileStream(file, FileMode.Open))
                {
                    // Create a TextRange that spans the entire document content
                    TextRange range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);

                    // Load the file stream into the TextRange, specifying the RTF data format
                    range.Load(fStream, System.Windows.DataFormats.Rtf);
                    this.FileName = file;
                }
            }
            else
            {
                if(MessageBox.Show("Arquivo não encontrado, deseja cria-lo?", "Criar Arquivo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Save(file);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender == mn_exit && Exit != null)
                Exit(null, null);
            if (sender == mn_open)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    FileName = "",
                    Title = "Abrir Arquivo",
                    Filter = "Arquivo de Texto | *.txt; *.t | Todos os Arquivos | *.*"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        using (FileStream fStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                        {
                            // Create a TextRange that spans the entire document content
                            TextRange range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);

                            // Load the file stream into the TextRange, specifying the RTF data format
                            range.Load(fStream, System.Windows.DataFormats.Rtf);
                            this.FileName = openFileDialog.FileName;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            else if (sender == mn_save_as)
                SaveAs();

            else if (sender == mn_save)
                Save(this.FileName);
        }
        void Save(string? fname)
        {
            if(fname == null)
            {
                SaveAs();
                return;
            }
            using (FileStream fileStream = new FileStream(fname, FileMode.Create))
            {
                // Create a TextRange from the RichTextBox document's start to end
                TextRange range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);

                // Save the TextRange content to the file stream using the Rtf data format
                range.Save(fileStream, DataFormats.Rtf);
                this.FileName = fname;
            }
        }
        void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                FileName = this.FileName,
                Title = "Abrir Arquivo",
                Filter = "Arquivo de Texto | *.txt; *.t | Todos os Arquivos | *.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                this.Save(saveFileDialog.FileName);
            }
        }
    }

    public class termToolEditViewModel : INotifyPropertyChanged
    {
        private string _FileName;

        public string FileName
        {
            get => _FileName;
            set
            {
                if (_FileName != value)
                {
                    _FileName = value;
                    OnPropertyChanged(); // Notify the UI that the value has changed
                }
            }
        }

        public string FileNameValid
        {
            get
            {
                if (FileName == null) return "Novo Arquivo";
                return FileName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
