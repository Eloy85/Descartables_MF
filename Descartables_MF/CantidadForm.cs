using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Descartables_MF
{
    public partial class CantidadForm : Form
    {
        public int CantidadSeleccionada { get; private set; }
        public CantidadForm()
        {
            InitializeComponent();
            numericUpDown1.KeyDown += NumericUpDown_KeyDown;
        }

        private void CantidadForm_Load(object sender, EventArgs e)
        {
            // Centra la ventana emergente en la posición especificada
            this.CenterToScreen();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            // Obtén la cantidad seleccionada del NumericUpDown
            CantidadSeleccionada = (int)numericUpDown1.Value;

            // Cierra la ventana
            DialogResult = DialogResult.OK;
            Close();
        }

        private void NumericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnAceptar_Click(sender, e);
            }
        }
    }
}
