using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HospitalManagementySite
{
    public partial class PatientAdmissionForm : Form
    {
        public PatientAdmissionForm()
        {
            InitializeComponent();
        }

        private void buttonAdmitExistingPatient_Click(object sender, EventArgs e)
        {
            PatientTriage patientTriage = new PatientTriage();
            patientTriage.Show();
        }
    }
}
