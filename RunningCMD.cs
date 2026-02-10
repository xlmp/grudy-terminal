using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Grudy
{
    public class ShellProcess
    {
        public event EventHandler<string?> OutPut;
        public event EventHandler Exit;
        string lineCommand { get; set; }
        string arguments { get; set; }
        string workingDirectory { get; set; }
        private string output_ { get; set; } = "";
        private BackgroundWorker bgWorker;
        public ShellProcess(string lineCommand, string arguments, string workingDirectory)
        {
            this.lineCommand = lineCommand;
            this.arguments = arguments;
            this.workingDirectory = workingDirectory;
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BgWorker_DoWork;
        }

        ~ShellProcess()
        {
            //bgWorker.CancelAsync();
            if (bgWorker != null)
                bgWorker.DoWork -= BgWorker_DoWork;
            bgWorker = null;
        }

        
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() => {
                if (OutPut != null)
                    OutPut(this, e.Data);

                if(e.Data == null)
                {
                    this.Process_Exited(sender, null);
                }
            });            

            output_ += $"{e.Data}\n";
            //Console.WriteLine($"OUTPUT: {e.Data}");
        }
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"ERROR: {e.Data}");
        }
        public void Start()
        {
            bgWorker.RunWorkerAsync();
        }
        public void Stop()
        {
            if(_process != null)
            {
                _process.CloseMainWindow();
                _process.Kill();
                //_process.ClosemainWin();
                _process.Close();
                _process = null;
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    this.Process_Exited(null, null);
                });
            }
            if (bgWorker != null)
                bgWorker.DoWork -= BgWorker_DoWork;
            bgWorker = null;
            
        }
        private void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (bgWorker.CancellationPending)
                return;

            this.StartWB();
        }
        Process _process { get; set; }  
        public string StartWB()
        {
            try
            {
                using (Process process = new Process())
                {
                    
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.FileName = "cmd.exe"; // Or any other executable
                    process.StartInfo.Arguments = $"/c {lineCommand} {arguments}"; // Comando exemplo
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.EnableRaisingEvents = true;

                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.ErrorDataReceived += Process_ErrorDataReceived;
                    process.Exited += Process_Exited;

                    this._process = process;

                    process.Start();

                    //string output = "";// process.StandardOutput.ReadToEnd();
                    // Start asynchronous reading of the output and error streams
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for the process to exit
                    process.WaitForExit();

                    return output_;
                }
                
            }
            catch (Exception ex)
            {
                return $"Erro Exception: {ex.Message}";
            }
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                this._process = null;
                if (Exit != null)
                {                    
                    Exit(this, e);
                }
            });
        }
    }
    public class RunningCMD
    {
        public string StartCmd(string cmd, string arguments, string workingDirectory)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.ErrorDataReceived += Process_ErrorDataReceived;

                    //process.Start();
                    //// Start asynchronous reading of the output and error streams
                    //process.BeginOutputReadLine();
                    //process.BeginErrorReadLine();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        //throw new Exception($"Erro Process: {error}");
                        return $"Erro Process: {error}";
                    }

                    return output;
                }
            }
            catch (Exception ex)
            {
                return StartCmd($"{cmd} {arguments}", workingDirectory);
                //return $"Erro Exception: {ex.Message}";
                //throw new Exception($"Erro Exception: {ex.Message}");
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"OUTPUT: {e.Data}");
        }

        public string StartCmd(string cmd, string workingDirectory) {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.WorkingDirectory = workingDirectory;
                startInfo.Arguments = $"/c {cmd}"; // Comando exemplo
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true; // Redireciona a saída
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                

                using (Process process = Process.Start(startInfo))
                {                   

                    // Lê a saída até o final
                    string output = process.StandardOutput.ReadToEnd();
                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.ErrorDataReceived += Process_ErrorDataReceived;

                    process.Start();
                    // Start asynchronous reading of the output and error streams
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    //Console.WriteLine(output); // Mostra no console do C#
                    return output;
                }
            }
            catch (Exception ex)
            {
                return $"Erro Exception: {ex.Message}";
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"ERROR: {e.Data}");
        }
    }
}
