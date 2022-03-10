using RoqueDiaz_Course_Project_part2_feb22;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;    
using System.Runtime.Serialization.Formatters.Binary;

namespace RoqueDiaz_Course_Project_part2_jan22
{
    public partial class MainForm : Form
    {
        // form level references
        const string FILENAME = "Employees.dat";
#pragma warning disable IDE0044 // Add readonly modifier
        private string healthIns;
#pragma warning restore IDE0044 // Add readonly modifier

        public object EmployeesListBox { get; private set; }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
       
        private void AddButton_Click(object sender, EventArgs e)
        {   
            //Add item to the employee list box from the form
            InputForm formInput = new InputForm();
            
            using(formInput)
            {
                DialogResult result = formInput.ShowDialog();
                // see if the form was canceled
                if (result == DialogResult.Cancel)
                    return;
                string fname = formInput.FirstNameTextBox.Text;
                string lname = formInput.LastNameTextBox.Text;
                string ssn = formInput.SSNTextBox.Text;
                string date = formInput.HireDateTextBox.Text;
                DateTime hireDate = DateTime.Parse(date);
                string healthIns = formInput.HealthInsTextBox.Text;
                double LifeIns = Double.Parse( formInput.LifeInsTextBox.Text);
                int vacation = Int32.Parse(formInput.VacationDaysTextBox.Text);

                Benefits benefits = new Benefits(healthIns, LifeIns, vacation);
                Employee emp;

                if (formInput.HourlyRadioButton.Checked)
                {   //get child items
                    float hourlyRate = float.Parse(formInput.Pay1TextBox.Text);
                    float hoursWorked = Convert.ToSingle(formInput.Pay2TextBox.Text);
                    // Polymorphism -- poly - many ; morph - change into
                    emp = new Hourly(fname, lname, ssn, hireDate, benefits, hourlyRate, hoursWorked);


                }
                else if (formInput.SalaryRadioButton.Checked)
                {
                    // get child items
                    double salary = Convert.ToDouble(formInput.Pay1TextBox.Text);

                    // polymorphism
                    emp = new Salary(fname, lname, ssn, hireDate, benefits, salary);

                }
                else
                {
                    MessageBox.Show("Error. Invalid Employee type.");
                    return; // end the method since we have an error
                }

                // add the employee object to the employees listbox
                EmployeeListBox.Items.Add(emp);

                // write all Employee objects to the file
                WriteEmpsToFile();

            }
           
        }
         
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            int itemNumber = EmployeeListBox.SelectedIndex;
            if (itemNumber > -1)
            {
                EmployeeListBox.Items.RemoveAt(itemNumber);
                ///write all employee objects to the file
                WriteEmpsToFile();
            }
            else
            {
                MessageBox.Show("Please select an employee to remove");
            }
        }

        private void DisplayButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Displaying all employees....");
            ReadEmpsFromFile();
        }
         
        private void PrintPaycheckButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Printing paychecks for all employees");
        }
        private void WriteEmpsToFile()
        {
            string fileName = "Employees.dat";
            
            // convert the listbox items to a generic list
            List<Employee> empList = new List<Employee>();

            foreach (Employee emp in EmployeeListBox.Items)
            {
                empList.Add(emp);
            }

            // open a pipe to the file and create a translator
            FileStream fs = new FileStream(FILENAME, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            // write the generic list to the file
            formatter.Serialize(fs, empList);

            // close the pipe
            fs.Close();



            //Write all employee objects to the file.
            StreamWriter sw = new StreamWriter(fileName);
            for (int i = 0; i < EmployeeListBox.Items.Count; i++)
            {
                Employee temp = (Employee)EmployeeListBox.Items[i];

                sw.WriteLine(temp.GetType().ToString() + ',' + temp.FirstName + "," + temp.LastName + "," + temp.SSN + "," + temp.HireDate.ToShortDateString()
                    + ',' + temp.BenefitsPackage.HealthInsurance +
                    ',' + temp.BenefitsPackage.LifeInsurance + ',' + temp.BenefitsPackage.Vacation);



            }
            sw.Close();

            MessageBox.Show("Employee were written to the file");
        }


        private void ReadEmpsFromFile()
        {
            // check to see if file exists
            if(File.Exists(FILENAME) && new FileInfo(FILENAME).Length > 0 )
            {
                // Create a pipe from the file and create the translator
                FileStream fs = new FileStream(FILENAME, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                //read all Employee objects from the file
                List<Employee> list = (List<Employee>)formatter.Deserialize(fs);

                // close the pipe
                fs.Close();

                // copy the Employee object into ours listbox
                foreach (Employee emp in list)
                    EmployeeListBox.Items.Add(emp);
            }
  
        }

        private void EmployeeListBox_DoubleClick(object sender, EventArgs e)
        {
            // edit selected employee in the listbox
            InputForm frmUpdate = new InputForm();

            using (frmUpdate)
            {
                frmUpdate.Text = "Employee Update Form";
                frmUpdate.SubmitsButton = "Update";
                int itemNumber = EmployeeListBox.SelectedIndex;

                if (itemNumber < 0)
                {
                    MessageBox.Show("Error. Invalid Employee.");
                    return;
                }

                Employee emp = (Employee)EmployeeListBox.Items[itemNumber];

                frmUpdate.FirstNameTextBox.Text = emp.FirstName;
                frmUpdate.LastNameTextBox.Text = emp.LastName;
                frmUpdate.SSNTextBox.Text = emp.SSN;
                frmUpdate.HireDateTextBox.Text = emp.HireDate.ToShortDateString();
                frmUpdate.HireDateTextBox.Text = emp.BenefitsPackage.HealthInsurance;
                frmUpdate.LifeInsTextBox.Text = emp.BenefitsPackage.LifeInsurance.ToString("C2");
                frmUpdate.VacationDaysTextBox.Text = emp.BenefitsPackage.Vacation.ToString();

                if (emp is Hourly hrly)
                {
                    frmUpdate.HourlyRadioButton.Checked = true;
                    frmUpdate.Pay1TextBox.Text = hrly.HourlyRate.ToString("N2");
                    frmUpdate.Pay2TextBox.Text = hrly.HourlyRate.ToString("N1");
                }
                else if (emp is Salary sal)
                {
                    frmUpdate.HourlyRadioButton.Checked = true;
                    frmUpdate.Pay1TextBox.Text = sal.AnnualSalary.ToString("N2");
                }

                DialogResult result = frmUpdate.ShowDialog();

                if (result == DialogResult.Cancel)
                    return;     // end the method since user cancel the update.

                EmployeeListBox.Items.RemoveAt(itemNumber);

                // get user's updated input and create and employee object
                string fname = frmUpdate.FirstNameTextBox.Text;
                string lname = frmUpdate.LastNameTextBox.Text;
                string ssn = frmUpdate.SSNTextBox.Text;
                string date = frmUpdate.HireDateTextBox.Text;
                DateTime hireDate = DateTime.Parse(date);
                string healthIns = frmUpdate.HealthInsTextBox.Text;
                double lifeIns = Double.Parse(frmUpdate.LifeInsTextBox.Text);
                int vacationDays = Int32.Parse(frmUpdate.VacationDaysTextBox.Text);

                Benefits benefits = new Benefits(healthIns, lifeIns,
                    vacationDays);

                if(frmUpdate.HourlyRadioButton.Checked)
                {
                    float rate = float.Parse(frmUpdate.Pay1TextBox.Text);
                    float hours = float.Parse(frmUpdate.Pay2TextBox.Text);
                    emp = new Hourly(fname, lname, ssn, hireDate, benefits, rate, hours);
                }
                else if(frmUpdate.SalaryRadioButton.Checked)
                {
                    double salary = Double.Parse(frmUpdate.Pay1TextBox.Text);
                    emp = new Salary(fname, lname, ssn, hireDate, benefits);
                }
    

                // add the updated Employee object to the employees listbox
                EmployeeListBox.Items.Add(emp);

                // write all of the updated employee objects to the file
                WriteEmpsToFile();

            }
        }

        public override bool Equals(object obj)
        {
            return obj is MainForm form &&
                   healthIns == form.healthIns;
        }

        public override int GetHashCode()
        {
            return -2070484435 + EqualityComparer<string>.Default.GetHashCode(healthIns);
        }
    }
}
