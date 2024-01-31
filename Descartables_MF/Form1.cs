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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtSearch.Enabled = false;
        }

        private void btn_open_file_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "Excel Sheet(*.xlsx)|*.xlsx|All Files(*.*)|*.*";
            if (op.ShowDialog() == DialogResult.OK)
            {
                string filepath = op.FileName;
                string con = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 8.0;HDR={1}'";
                con = string.Format(con, filepath, "yes");
                OleDbConnection excelconnection = new OleDbConnection(con);
                excelconnection.Open();
                DataTable dtexcel = excelconnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string excelsheet = dtexcel.Rows[0]["TABLE_NAME"].ToString();
                OleDbCommand com = new OleDbCommand("Select * from [" + excelsheet + "]", excelconnection);
                OleDbDataAdapter oda = new OleDbDataAdapter(com);
                DataTable dt = new DataTable();
                oda.Fill(dt);
                excelconnection.Close();
                label_file_name.Text = Path.GetFileName(filepath);
                originalDataTable = dt;  // Asigna originalDataTable al DataTable cargado
                dataGridView1.DataSource = dt;
                txtSearch.Enabled = true;

                // Agrega las columnas de dataGridView1 a dataGridView2
                dataGridView2.Columns.Clear(); // Limpia las columnas existentes en dataGridView2
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    dataGridView2.Columns.Add((DataGridViewColumn)column.Clone());
                }
            }
        }

        private void btn_add_product_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                if (dataGridView2.Columns.Count == 0)
                {
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        dataGridView2.Columns.Add((DataGridViewColumn)col.Clone());
                    }
                }
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                DataGridViewRow newRow = (DataGridViewRow)selectedRow.Clone();

                for (int i = 0; i < selectedRow.Cells.Count; i++)
                {
                    newRow.Cells[i].Value = selectedRow.Cells[i].Value;
                }
                dataGridView2.Rows.Add(newRow);
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
                    // Obtiene la fila seleccionada
                    DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                    // Clona la fila seleccionada
                    DataGridViewRow newRow = (DataGridViewRow)selectedRow.Clone();

                    // Copia los valores de la fila seleccionada a la nueva fila
                    for (int i = 0; i < selectedRow.Cells.Count; i++)
                    {
                        newRow.Cells[i].Value = selectedRow.Cells[i].Value;
                    }

                    // Agrega la nueva fila a dataGridView2
                    dataGridView2.Rows.Add(newRow);
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
            // Verificar si hay datos en dataGridView2
            if (dataGridView2.Rows.Count > 0)
            {
                PrintDocument pd = new PrintDocument();

                // Asocia el evento de impresión
                pd.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);

                // Configura el tamaño del papel (por ejemplo, carta)
                pd.DefaultPageSettings.PaperSize = new PaperSize("CustomSize", 314, 390); // Ancho x Alto en cien milésimas de pulgada

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

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Verifica si hay datos en dataGridView2
            if (dataGridView2 != null && dataGridView2.Rows.Count > 0)
            {
                // Dibuja una imagen en el encabezado
                Image headerImage = Properties.Resources.LogoEmpresa;
                e.Graphics.DrawImage(headerImage, new PointF(20, 10));  // Ajusta la posición según tus necesidades

                int y = 200; // Posición vertical inicial después de la imagen

                // Imprime los encabezados de las columnas
                int xHeader = 20;
                foreach (DataGridViewColumn column in dataGridView2.Columns)
                {
                    e.Graphics.DrawString(column.HeaderText, new Font("Arial", 10, FontStyle.Bold), Brushes.Black, new PointF(xHeader, y - 30));
                    xHeader += 60; // Ajusta la posición horizontal para la siguiente columna
                }

                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    // Verifica si la fila y las celdas no son null
                    if (row != null && row.Cells.Count > 0)
                    {
                        int x = 20; // Posición horizontal inicial
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            // Verifica si el valor de la celda no es null
                            if (cell.Value != null)
                            {
                                e.Graphics.DrawString(cell.Value.ToString(), dataGridView2.Font, Brushes.Black, new PointF(x, y));
                                x += 60; // Ajusta la posición horizontal para la siguiente celda
                            }
                        }
                        y += 20; // Ajusta la posición vertical para la siguiente fila
                    }
                }

                string nombreEmpleado = txt_employee_name.Text;
                DateTime fecha = DateTime.Now;
                e.Graphics.DrawString("Empleado: " + nombreEmpleado, new Font("Arial", 10, FontStyle.Regular), Brushes.Black, new PointF(20, y));
                y += 20;
                e.Graphics.DrawString(fecha.ToString(), new Font("Arial", 10, FontStyle.Regular), Brushes.Black, new PointF(20, y));
                y += 60;
                e.Graphics.DrawString("Firma: ______________________________", new Font("Arial", 10, FontStyle.Regular), Brushes.Black, new PointF(20, y));

                // Configura el tamaño del ticket en píxeles
                int ticketWidthInPixels = 302;
                int ticketHeightInPixels = 375;

                // Calcula el factor de escala para ajustar el contenido al tamaño deseado
                float scaleX = (float)ticketWidthInPixels / e.PageSettings.PrintableArea.Width;
                float scaleY = (float)ticketHeightInPixels / e.PageSettings.PrintableArea.Height;

                // Aplica la escala al contenido del ticket
                e.Graphics.ScaleTransform(scaleX, scaleY);

                // Configura el tamaño del papel
                e.PageSettings.PaperSize = new PaperSize("Custom", 314, 390); // Ancho x Alto en cien milésimas de pulgada

                // Calcula la posición de inicio para centrar el contenido en la página
                int startX = (e.PageBounds.Width - dataGridView2.Width) / 2;
                int startY = (e.PageBounds.Height - dataGridView2.Height) / 2;

                // Dibuja el contenido (dataGridView2) en la posición calculada
                e.Graphics.TranslateTransform(startX, startY);
                dataGridView2.DrawToBitmap(new Bitmap(dataGridView2.Width, dataGridView2.Height), new Rectangle(0, 0, dataGridView2.Width, dataGridView2.Height));
                e.Graphics.TranslateTransform(-startX, -startY);
            }
            else
            {
                MessageBox.Show("No hay datos para imprimir en el ticket.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private void btn_clean_fields_Click(object sender, EventArgs e)
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
    }
}
