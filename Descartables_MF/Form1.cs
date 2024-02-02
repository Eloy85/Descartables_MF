using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Drawing.Printing;
using System.Data.Common;

namespace Descartables_MF
{
    public partial class Form1 : Form
    {
        private DataTable originalDataTable;  // Declaración de la variable
        public Form1()
        {
            InitializeComponent();
            dataGridView1.CellClick += dataGridView_CellClick;
            dataGridView2.CellClick += dataGridView_CellClick;
            txtSearch.TextChanged += txtSearch_TextChanged;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.KeyDown += dataGridView1_KeyDown;
            txtSearch.KeyPress += txtSearch_KeyPress;
            dataGridView1.KeyUp += dataGridView1_KeyUp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtSearch.Enabled = false;
            this.CenterToScreen();
            // Especifica la ruta del archivo que deseas cargar
            string filePath = Path.Combine(Application.StartupPath, "output_form_003_Mantenimiento_De_Productos.xlsx");

            // Verifica si el archivo existe antes de intentar cargarlo
            if (File.Exists(filePath))
            {
                // Carga automáticamente el archivo en el DataGridView
                LoadExcelFile(filePath);
            }
        }

        // Método para cargar el archivo Excel en el DataGridView
        private void LoadExcelFile(string filePath)
        {
            string con = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 8.0;HDR={1}'";
            con = string.Format(con, filePath, "yes");
            OleDbConnection excelconnection = new OleDbConnection(con);
            excelconnection.Open();
            DataTable dtexcel = excelconnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            string excelsheet = dtexcel.Rows[0]["TABLE_NAME"].ToString();
            OleDbCommand com = new OleDbCommand("Select * from [" + excelsheet + "]", excelconnection);
            OleDbDataAdapter oda = new OleDbDataAdapter(com);
            DataTable dt = new DataTable();
            oda.Fill(dt);
            excelconnection.Close();

            // Asigna la DataTable cargada al DataGridView
            originalDataTable = dt;  // originalDataTable es la DataTable original cargada en dataGridView1
            dataGridView1.DataSource = dt;

            // Agrega las columnas de dataGridView1 a dataGridView2
            dataGridView2.Columns.Clear(); // Limpia las columnas existentes en dataGridView2
            if (dataGridView2.Columns.Count == 0)
            {
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    dataGridView2.Columns.Add((DataGridViewColumn)col.Clone());
                }

                // Agrega la columna "Cantidad" al final
                DataGridViewColumn cantidadColumn = new DataGridViewColumn(new DataGridViewTextBoxCell());
                cantidadColumn.HeaderText = "Cantidad";
                cantidadColumn.Name = "Cantidad";
                dataGridView2.Columns.Add(cantidadColumn);
            }

            // Actualiza la etiqueta con el nombre del archivo cargado
            label_file_name.Text = Path.GetFileName(filePath);

            // Habilita el campo de búsqueda
            txtSearch.Enabled = true;
        }

        private void btn_open_file_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "Excel Sheet(*.xlsx)|*.xlsx|All Files(*.*)|*.*";
            if (op.ShowDialog() == DialogResult.OK)
            {
                string filepath = op.FileName;
                LoadExcelFile(filepath);
            }
        }

        private void btn_add_product_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                using (CantidadForm cantidadForm = new CantidadForm())
                {
                    // Obtén la posición central de Form1
                    Point centerPoint = new Point(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);

                    // Calcula la posición para centrar CantidadForm
                    cantidadForm.StartPosition = FormStartPosition.Manual;
                    cantidadForm.Left = centerPoint.X - cantidadForm.Width / 2;
                    cantidadForm.Top = centerPoint.Y - cantidadForm.Height / 2;

                    if (cantidadForm.ShowDialog() == DialogResult.OK)
                    {
                        int cantidadSeleccionada = cantidadForm.CantidadSeleccionada;

                        DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                        DataGridViewRow newRow = (DataGridViewRow)selectedRow.Clone();

                        for (int i = 0; i < selectedRow.Cells.Count; i++)
                        {
                            newRow.Cells[i].Value = selectedRow.Cells[i].Value;
                        }

                        // Verifica si la columna "Cantidad" existe en dataGridView2
                        DataGridViewColumn cantidadColumn = dataGridView2.Columns["Cantidad"];
                        if (cantidadColumn != null)
                        {
                            int cantidadColumnIndex = cantidadColumn.Index;

                            // Si la columna existe, verifica si la nueva fila tiene la celda "Cantidad"
                            if (cantidadColumnIndex < newRow.Cells.Count)
                            {
                                newRow.Cells[cantidadColumnIndex].Value = cantidadSeleccionada;
                            }
                            else
                            {
                                // Si la celda "Cantidad" no existe, agrégala
                                newRow.Cells.Add(new DataGridViewTextBoxCell { Value = cantidadSeleccionada });
                            }
                        }
                        else
                        {
                            // Si la columna "Cantidad" no existe en dataGridView2, agrégala
                            DataGridViewCell cantidadCell = new DataGridViewTextBoxCell { Value = cantidadSeleccionada };
                            newRow.Cells.Add(cantidadCell);
                        }

                        dataGridView2.Rows.Add(newRow);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una fila en la tabla de arriba", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;  // Indica que la tecla Enter ha sido manejada

                // Verifica que haya al menos una fila seleccionada en dataGridView1
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    using (CantidadForm cantidadForm = new CantidadForm())
                    {
                        // Obtén la posición central de Form1
                        Point centerPoint = new Point(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);

                        // Calcula la posición para centrar CantidadForm
                        cantidadForm.StartPosition = FormStartPosition.Manual;
                        cantidadForm.Left = centerPoint.X - cantidadForm.Width / 2;
                        cantidadForm.Top = centerPoint.Y - cantidadForm.Height / 2;

                        if (cantidadForm.ShowDialog() == DialogResult.OK)
                        {
                            int cantidadSeleccionada = cantidadForm.CantidadSeleccionada;

                            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                            DataGridViewRow newRow = (DataGridViewRow)selectedRow.Clone();

                            for (int i = 0; i < selectedRow.Cells.Count; i++)
                            {
                                newRow.Cells[i].Value = selectedRow.Cells[i].Value;
                            }

                            // Verifica si la columna "Cantidad" existe en dataGridView2
                            DataGridViewColumn cantidadColumn = dataGridView2.Columns["Cantidad"];
                            if (cantidadColumn != null)
                            {
                                int cantidadColumnIndex = cantidadColumn.Index;

                                // Si la columna existe, verifica si la nueva fila tiene la celda "Cantidad"
                                if (cantidadColumnIndex < newRow.Cells.Count)
                                {
                                    newRow.Cells[cantidadColumnIndex].Value = cantidadSeleccionada;
                                }
                                else
                                {
                                    // Si la celda "Cantidad" no existe, agrégala
                                    newRow.Cells.Add(new DataGridViewTextBoxCell { Value = cantidadSeleccionada });
                                }
                            }
                            else
                            {
                                // Si la columna "Cantidad" no existe en dataGridView2, agrégala
                                DataGridViewCell cantidadCell = new DataGridViewTextBoxCell { Value = cantidadSeleccionada };
                                newRow.Cells.Add(cantidadCell);
                            }

                            dataGridView2.Rows.Add(newRow);
                        }
                    }
                }
            }
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGridView = (DataGridView)sender;

            if (e.RowIndex >= 0)
            {
                dataGridView.Rows[e.RowIndex].Selected = true;
            }
        }

        private void btn_print_ticket_Click(object sender, EventArgs e)
        {
            try
            {
                // Verificar si hay datos en dataGridView2
                if (dataGridView2.Rows.Count > 0)
                {
                    PrintDocument pd = new PrintDocument();

                    // Asocia el evento de impresión
                    pd.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);

                    // Configura el tamaño del papel (por ejemplo, carta)
                    pd.DefaultPageSettings.PaperSize = new PaperSize("A6", 410, 580); // Ancho x Alto en cien milésimas de pulgada

                    // Muestra el cuadro de diálogo de impresión
                    PrintDialog printDialog = new PrintDialog();
                    printDialog.Document = pd;

                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        pd.Print();
                    }
                }
                else
                {
                    MessageBox.Show("No hay datos para imprimir en el ticket.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (System.ComponentModel.Win32Exception win32Ex)
            {
                MessageBox.Show($"Error al imprimir: {win32Ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Puedes realizar otras acciones según tus necesidades
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Otro error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Puedes manejar otros tipos de excepciones aquí
            }
        }


        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font fuente = new Font("Arial", 10, FontStyle.Regular);
            // Verifica si hay datos en dataGridView2
            if (dataGridView2 != null && dataGridView2.Rows.Count > 0)
            {
                // Dibuja una imagen en el encabezado
                Image headerImage = Properties.Resources.LogoEmpresa_header;
                e.Graphics.DrawImage(headerImage, new PointF(140, 5));  // Ajusta la posición según tus necesidades

                int y = 110; // Posición vertical inicial después de la imagen

                e.Graphics.DrawString("Descartables", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, new PointF(150, y - 20));

                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    // Verifica si la fila y las celdas no son null
                    if (row != null && row.Cells.Count > 0)
                    {
                        float x = 20; // Posición horizontal inicial
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            // Verifica si el valor de la celda no es null
                            if (cell.Value != null)
                            {
                                // Configura el ancho máximo para cada celda
                                int cellWidth = 330;

                                // Divide el contenido de la celda en líneas según el ancho máximo
                                List<string> lines = GetLines(cell.Value.ToString(), cellWidth, fuente, e);

                                // Imprime cada línea
                                foreach (string line in lines)
                                {
                                    if (cell.OwningColumn.Name == "Cantidad")  // Verifica si es la columna de cantidades
                                    {
                                        // Agrega "x" antes de la cantidad
                                        e.Graphics.DrawString("(x" + line.Trim() + ")", fuente, Brushes.Black, new PointF(x, y));
                                    }
                                    else
                                    {
                                        e.Graphics.DrawString(line, fuente, Brushes.Black, new PointF(x, y));
                                    }
                                    // Ajusta la posición para la siguiente línea
                                    if (x + e.Graphics.MeasureString(line, fuente).Width <= cellWidth)
                                    {
                                        x += e.Graphics.MeasureString(line, fuente).Width;
                                    }
                                    else
                                    {
                                        x = 55;
                                        y += 20;
                                    }
                                }
                            }
                        }
                        y += 20; // Ajusta la posición vertical para la siguiente fila
                    }
                }

                string nombreEmpleado = txt_employee_name.Text;
                DateTime fecha = DateTime.Now;
                e.Graphics.DrawString("Empleado: " + nombreEmpleado.ToUpper(), fuente, Brushes.Black, new PointF(20, y - 10));
                y += 10;
                e.Graphics.DrawString(fecha.ToString(), fuente, Brushes.Black, new PointF(20, y));
                y += 60;
                e.Graphics.DrawString("Firma: ______________________________", fuente, Brushes.Black, new PointF(20, y));

                // Configura el tamaño del ticket en píxeles
                int ticketWidthInPixels = 500;
                int ticketHeightInPixels = 700;

                // Calcula el factor de escala para ajustar el contenido al tamaño deseado
                float scaleX = (float)ticketWidthInPixels / e.PageSettings.PrintableArea.Width;
                float scaleY = (float)ticketHeightInPixels / e.PageSettings.PrintableArea.Height;

                // Aplica la escala al contenido del ticket
                e.Graphics.ScaleTransform(scaleX, scaleY);

                // Configura el tamaño del papel
                e.PageSettings.PaperSize = new PaperSize("Custom", 315, 389); // Ancho x Alto en cien milésimas de pulgada

                // Calcula la posición de inicio para centrar el contenido en la página
                int startX = (e.PageBounds.Width - dataGridView2.Width) / 2;
                int startY = (e.PageBounds.Height - dataGridView2.Height) / 2;

                // Dibuja el contenido (dataGridView2) en la posición calculada
                e.Graphics.TranslateTransform(startX, startY);
                dataGridView2.DrawToBitmap(new Bitmap(dataGridView2.Width, dataGridView2.Height), new Rectangle(0, 0, dataGridView2.Width, dataGridView2.Height));
                e.Graphics.TranslateTransform(-startX, -startY);

                //Limpiar los campos después de imprimir
                CleanFields();
            }
            else
            {
                MessageBox.Show("No hay datos para imprimir en el ticket.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<string> GetLines(string text, int maxWidth, Font fuente, PrintPageEventArgs e)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');
            string currentLine = "";
            foreach (string word in words)
            {
                if (e.Graphics.MeasureString(currentLine + word, fuente).Width <= maxWidth)
                {
                    currentLine += word + " ";
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word + " ";
                }
            }

            lines.Add(currentLine);

            return lines;
        }

        private void CleanFields()
        {
            txt_employee_name.Clear();

            if (dataGridView2.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    dataGridView2.Rows.Clear();
                }

                RestoreOriginalData();
            }
        }

        private void btn_clean_fields_Click(object sender, EventArgs e)
        {
            CleanFields();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string[] searchTerms = txtSearch.Text.ToLower().Split(' ');

            if (searchTerms.Length > 0)
            {
                BindingSource bs = new BindingSource();
                bs.DataSource = originalDataTable;  // originalDataTable es la DataTable original cargada en dataGridView1

                List<string> filters = new List<string>();

                // Construye una lista de fragmentos de búsqueda
                foreach (string term in searchTerms)
                {
                    filters.Add($"Convert({originalDataTable.Columns[1].ColumnName}, 'System.String') LIKE '%{term}%'");
                }

                // Combina los fragmentos con el operador AND
                string combinedFilter = string.Join(" AND ", filters);

                // Aplica el filtro
                bs.Filter = combinedFilter;

                dataGridView1.DataSource = bs;
            }
            else
            {
                // Si no hay fragmentos de búsqueda, restaura los datos originales
                RestoreOriginalData();
            }
        }

        private void RestoreOriginalData()
        {
            // Restaura los datos originales
            if (originalDataTable != null)
            {
                dataGridView1.DataSource = originalDataTable;
                txtSearch.Text = "";
            }
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Evita que se procese el Enter por defecto
                dataGridView1.Focus();
            }
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                txtSearch.Focus();
            }
        }
    }
}