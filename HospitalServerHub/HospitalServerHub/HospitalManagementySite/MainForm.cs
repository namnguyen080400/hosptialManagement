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
    public partial class MainForm : Form
    {
        User currUser = LoginData.LoginUser;

        public MainForm()
        {
            InitializeComponent();

            InitializeForm();


        }

        private void InitializeForm()
        {


            button_dataAnalytics.Visible = currUser.AccessLevel != 4;
            button_medicalInventory.Visible = currUser.AccessLevel != 4;

            string type;
            switch (currUser.AccessLevel)
            {
                case 1:
                    type = "doctor";
                    break;
                case 2:
                    type = "nurse";
                    break;
                case 3:
                    type = "staff";
                    break;

                default:
                    type = "patient";
                    break;


            }
            label_loginType.Text = $"[{type}]";

            label_greet.Text = $"Hello, {currUser.FirstName} !";

        }

        private void button_appointment_Click(object sender, EventArgs e)
        {
            AppointmentForm appointmentForm = new AppointmentForm();

            appointmentForm.Show();
            this.Hide();
        }

        private void button_patientManagement_Click(object sender, EventArgs e)
        {

            PatientManagementForm patientManagementForm = new PatientManagementForm();

            patientManagementForm.Show();
            this.Hide();

        }

        private void button_signOut_Click(object sender, EventArgs e)
        {
            LoginData.LoginUser = null;

            MessageBox.Show("Logout successfully!");
            LoginForm2 loginForm = new LoginForm2();
            loginForm.Show();
            this.Hide();
        }

        private void button_medicalInventory_Click(object sender, EventArgs e)
        {
            if (currUser.AccessLevel != 3)
            {
                MessageBox.Show("Only staff are allowed to manage medical inventory");
            }
            else
            {
                MedicalInventoryForm medicalInventoryForm = new MedicalInventoryForm();
                medicalInventoryForm.Show();
            }
        }

        private void buttonPatientAdmission_Click(object sender, EventArgs e)
        {
            if (currUser.AccessLevel != 1 || currUser.AccessLevel != 2 || currUser.AccessLevel != 3)
            {
                MessageBox.Show("Only doctor, nurse, and staff can admit patient");
            }
            else
            {
                PatientAdmissionForm patientAdmissionForm = new PatientAdmissionForm();
                patientAdmissionForm.Show();
            }
        }
    }
}
