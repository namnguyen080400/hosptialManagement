using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;

namespace HospitalManagementySite
{
    public partial class AppointmentForm : Form
    {
        private HospitalManagementDBEntities1 context;
        private HubConnection connection;

        public AppointmentForm()
        {
            InitializeComponent();
            context = new HospitalManagementDBEntities1();
            InitializeSignalR();
            LoadPatients();
            LoadDoctors();
            LoadAppointment();
        }


       

        private void LoadPatients()
        {
            // Load patients into comboBoxPatient
            var patients = context.Patients.Select(p => new { p.PatientId }).ToList();
            comboBoxPatient.DataSource = patients;
            comboBoxPatient.DisplayMember = "PatientId";
            comboBoxPatient.ValueMember = "PatientId";

        }

        private void LoadDoctors()
        {
            // Load patients into comboBoxPatient
            var doctors = context.Doctors.Select(p => new { p.DoctorId }).ToList();
            comboBoxDoctor.DataSource = doctors;

            comboBoxDoctor.DisplayMember = "DoctorId";
            comboBoxDoctor.ValueMember = "DoctorId";
            MessageBox.Show($"{ doctors.Count}");
        
        }



        private async void InitializeSignalR()
        {
            connection = new HubConnectionBuilder()
                   .WithUrl("http://localhost:5041/appointmentHub")
                   .Build();

            connection.On<string>("ReceiveAppointmentUpdate", (message) =>
            {
                Invoke((Action)(() =>
                {
                    MessageBox.Show(message);
                    LoadAppointment();
                }));
            });

            try
            {
                await connection.StartAsync();
                MessageBox.Show("Connection to the hub was successful");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to SignalR hub: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAppointment()
        {
            using (var context = new HospitalManagementDBEntities1())
            {
                var appointments = context.Appointments.ToList();
                dataGridView1.DataSource = appointments;
            }
        }

        private void button_submit_Click(object sender, EventArgs e)
        {
                try
                {

                // Validate selected items
                if (comboBoxPatient.SelectedItem == null)
                {
                    MessageBox.Show("Please select a patient.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (comboBoxDoctor.SelectedItem == null)
                {
                    MessageBox.Show("Please select a doctor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int patientId = Convert.ToInt32(comboBoxPatient.SelectedValue);
                int doctorId = Convert.ToInt32(comboBoxDoctor.SelectedValue);

                var newAppointment = new Appointment
                {
                    PatientId = patientId,
                    DoctorId = doctorId,
                    AppointmentDate = monthCalendar1.SelectionStart,
                    Status = "Scheduled",
                    Description = richTextBox1.Text,
                };

                context.Appointments.Add(newAppointment);

                context.SaveChanges();
                MessageBox.Show("Appointment saved successfully.");

                // SignalR code to notify about appointment update
                SendAppointmentUpdate(newAppointment.DoctorId.ToString());
                    LoadAppointment();
                }
                catch (DbEntityValidationException ex)
                {
                    // Display validation errors
                    StringBuilder sb = new StringBuilder();
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            sb.AppendLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                    MessageBox.Show($"Validation errors:\n{sb.ToString()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving appointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            
        }

        private async void SendAppointmentUpdate(string doctorId)
        {
            try
            {
                await connection.InvokeAsync("SendAppointmentUpdate", doctorId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending appointment update: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
