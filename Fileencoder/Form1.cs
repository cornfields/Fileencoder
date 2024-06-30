using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Fileencoder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtFolderPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnGenerateScript_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFolderPath.Text))
            {
                MessageBox.Show("Please select a folder first.");
                return;
            }

            string sourceDirectory = txtFolderPath.Text;
            string outputScriptPath = Path.Combine(sourceDirectory, "OutputScript.ps1");

            StringBuilder psScript = new StringBuilder();
            psScript.AppendLine("$base64Strings = @{");

            EncodeDirectory(sourceDirectory, psScript, sourceDirectory);

            psScript.AppendLine("}");

            // The Powershell script body
            psScript.AppendLine("foreach ($key in $base64Strings.Keys) {");
            psScript.AppendLine("  $directoryPath = [System.IO.Path]::GetDirectoryName($key)");
            psScript.AppendLine("  if ($directoryPath -ne \"\" -and $directoryPath -ne $null) {");
            psScript.AppendLine("    if (-not (Test-Path $directoryPath)) {");
            psScript.AppendLine("      New-Item -ItemType Directory -Path $directoryPath");
            psScript.AppendLine("    }");
            psScript.AppendLine("  }");
            psScript.AppendLine("  $filePath = $key");
            psScript.AppendLine("  $base64String = $base64Strings[$key]");
            psScript.AppendLine("  $bytes = [System.Convert]::FromBase64String($base64String)");
            psScript.AppendLine("  [System.IO.File]::WriteAllBytes($filePath, $bytes)");
            psScript.AppendLine("}");

            File.WriteAllText(outputScriptPath, psScript.ToString());
            MessageBox.Show("PowerShell script generated successfully.");
        }



        private void EncodeDirectory(string path, StringBuilder psScript, string baseDirectory)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                EncodeDirectory(directory, psScript, baseDirectory);
            }

            foreach (string file in Directory.GetFiles(path))
            {
                string base64String = Convert.ToBase64String(File.ReadAllBytes(file));
                string relativePath = GetRelativePath(baseDirectory, file).Replace("\\", "/");
                psScript.AppendLine($"  \"{relativePath}\" = \"{base64String}\";");
            }
        }

        private static string GetRelativePath(string basePath, string fullPath)
        {
            Uri baseUri = new Uri(basePath.EndsWith("\\") ? basePath : basePath + "\\");
            Uri fullUri = new Uri(fullPath);
            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', '\\');
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tract0r\nhttps://x.com/Tract0r_\n@2024", "About");
        }

    }
}
