using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Finisar.SQLite;
using System.Drawing.Printing;
using System.Diagnostics;

namespace SQLiteAssistant
{
    public partial class frmSQLite : Form
    {
        private string DB_NAME = null;
        private SQLiteDataAdapter dataAdapter = null;
        private DataSet dataSet = null;
        private DataGridViewPrinter userDataGridViewPrinter = null;

        //<summary>
        //Constructor
        //</summary>
        public frmSQLite()
        {
            InitializeComponent();
            cmbColumnType.Items.Add("INTEGER");
            cmbColumnType.Items.Add("VARCHAR");
            cmbColumnType.Items.Add("TEXT");
            cmbColumnType.SelectedIndex = 0;
        }

        //<summary>
        //Delete a table from the DB
        //</summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (DB_NAME == null)
                {
                    MessageBox.Show("Open an existing database or Create a new database", "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string connString = String.Format("Data Source={0};New=False;Version=3", DB_NAME);

                SQLiteConnection sqlconn = new SQLiteConnection(connString);

                sqlconn.Open();

                string CommandText = String.Format("drop table {0};", tablecombobox.Text);

                SQLiteCommand SQLiteCommand = new SQLiteCommand(CommandText, sqlconn);
                SQLiteCommand.ExecuteNonQuery();

                sqlconn.Close();

                reload_tables();

                sblblstatus.Text = "SQLite GUI: " + tablecombobox.Text + " table deleted";
                tmrTimer.Enabled = true;
            }
            catch (SQLiteException sqlex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + sqlex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //<summary>
        //Reload the tables in the Combo Box
        //</summary>
        private void reload_tables()
        {
            try
            {
                tablecombobox.Items.Clear();

                DataSet ds = new DataSet();

                if (DB_NAME == null)
                {
                    MessageBox.Show("Open an existing database or Create a new database", "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string connString = String.Format("Data Source={0};New=False;Version=3", DB_NAME);

                SQLiteConnection sqlconn = new SQLiteConnection(connString);

                sqlconn.Open();

                string CommandText = "SELECT name FROM sqlite_master;";

                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(CommandText, sqlconn);
                dataAdapter.Fill(ds);

                DataRowCollection dataRowCol = ds.Tables[0].Rows;

                foreach (DataRow dr in dataRowCol)
                {
                    tablecombobox.Items.Add(dr["name"]);
                }

                if (tablecombobox.Items.Count > 0)
                {
                    tablecombobox.SelectedIndex = 0;
                    btnDelete.Enabled = true;
                }
                else
                {
                    tablecombobox.Text = " ";
                    btnDelete.Enabled = false;
                }

                sqlconn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return;
        }
        //<summary>
        //On change column type
        //</summary>
        private void cmbColumnType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        //<summary>
        //Create a new table in the DB
        //</summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder query = new StringBuilder();

                if (txtTableName.Enabled)
                {
                    if (txtTableName.Text.Length <= 0)
                    {
                        sblblstatus.Text = "SQLite GUI: Table Name cannot be empty!!!";
                        txtTableName.Focus();
                        tmrTimer.Enabled = true;
                        return;
                    }

                    txtQuery.Text = "";
                    string createStr = String.Format("CREATE TABLE {0}(\r\n  ", txtTableName.Text);
                    query.Append(createStr);
                }
                else
                {
                    query.Append(", \r\n  ");
                }

                if (txtColumnName.Text.Length <= 0)
                {
                    sblblstatus.Text = "SQLite GUI: Column Name cannot be empty!!!";
                    txtColumnName.Focus();
                    tmrTimer.Enabled = true;
                    return;
                }
                txtTableName.Enabled = false;

                query.Append(txtColumnName.Text);
                query.Append(" ");
                if (chkPrimary.Enabled)
                {
                    if (chkPrimary.CheckState == CheckState.Checked)
                    {
                        cmbColumnType.SelectedIndex = 0;
                        lblPrimarykey.Text = txtColumnName.Text;
                        query.Append("INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE");
                        chkPrimary.Enabled = false;
                    }
                }
                else 
                {
                    query.Append(cmbColumnType.Text);
                    if (txtColumnSize.Text.Length <= 0 || txtColumnSize.Text == "0")
                    {
                        if (cmbColumnType.Text == "INTEGER") query.Append("(11)");
                        else if (cmbColumnType.Text == "VARCHAR") query.Append("(100)");
                        else if (cmbColumnType.Text == "TEXT") query.Append("(1000)");
                    }
                    else query.Append("(" + txtColumnSize.Text + ")");

                    if (txtColumnDefault.Text.Length <= 0 || txtColumnDefault.Text == "NULL") 
                        query.Append("  DEFAULT NULL");
                    else query.Append(" NOT NULL DEFAULT '" + txtColumnDefault.Text + "'");
                }

                txtQuery.Text += query.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        //<summary>
        //Create a new table in the DB
        //</summary>
        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {

                if (DB_NAME == null)
                {
                    MessageBox.Show("Open an existing database or Create a new database", "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //txtQuery.Text += ",\r\n  PRIMARY KEY (" + lblPrimarykey.Text + ")\r\n);";
                txtQuery.Text += "\r\n);";

                string connString = String.Format("Data Source={0};New=False;Version=3", DB_NAME);

                SQLiteConnection sqlconn = new SQLiteConnection(connString);

                sqlconn.Open();

                string CommandText = String.Format("{0}", txtQuery.Text);

                SQLiteCommand SQLiteCommand = new SQLiteCommand(CommandText, sqlconn);
                SQLiteCommand.ExecuteNonQuery();

                sqlconn.Close();

                txtTableName.Enabled = true;

                reload_tables();

                sblblstatus.Text = "SQLite GUI: the Table " + txtTableName.Text + " has been created";
                tmrTimer.Enabled = true;
            }
            catch (SQLiteException sqlex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + sqlex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtColumnName.Text = "";
            txtTableName.Text = "";
            cmbColumnType.SelectedIndex = 0;
            txtQuery.Clear();
            txtTableName.Enabled = true;
            chkPrimary.Enabled = true;
            chkPrimary.CheckState = CheckState.Checked;
        }

        //<summary>
        //Executes a custom Query
        //</summary>
        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {

                if (DB_NAME == null)
                {
                    MessageBox.Show("Open an existing database or Create a new database", "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string connString = String.Format("Data Source={0};New=False;Version=3", DB_NAME);

                SQLiteConnection sqlconn = new SQLiteConnection(connString);

                sqlconn.Open();

                string CommandText = String.Format("{0}", txtCustomQuery.Text);

                SQLiteCommand SQLiteCommand = new SQLiteCommand(CommandText, sqlconn);
                SQLiteCommand.ExecuteNonQuery();

                sqlconn.Close();

                sblblstatus.Text = "SQLite GUI: the query has been executed successfully!";
                tmrTimer.Enabled = true;
            }
            catch (SQLiteException sqlex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + sqlex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //<summary>
        //Displays the contents of the table in the DataGridView
        //</summary>
        private void tablecombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (DB_NAME == null)
                {
                    MessageBox.Show("Open an existing database or Create a new database", "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                this.dataSet = new DataSet();
                string connString = String.Format("Data Source={0};New=False;Version=3", DB_NAME);

                SQLiteConnection sqlconn = new SQLiteConnection(connString);
                sqlconn.Open();

                //string CommandText = String.Format("SELECT SUBSTRING_INDEX(*, '...', 20) FROM {0};", tablecombobox.Text);
                string CommandText = String.Format("SELECT * FROM {0};", tablecombobox.Text);

                this.dataAdapter = new SQLiteDataAdapter(CommandText, sqlconn);
                SQLiteCommandBuilder builder = new SQLiteCommandBuilder(this.dataAdapter);

                this.dataAdapter.Fill(this.dataSet, tablecombobox.Text);

                userDataGridView.DataSource = this.dataSet;
                userDataGridView.DataMember = tablecombobox.Text;

            }
            catch (SQLiteException sqlex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + sqlex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //<summary>
        //Update the changes in the DataGridView to the DB through the Datset and Data Adapter
        //</summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (DB_NAME == null)
                {
                    MessageBox.Show("Open an existing database or Create a new database", "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (this.dataSet.HasChanges())
                {
                    this.dataAdapter.Update(this.dataSet, tablecombobox.Text);
                    sblblstatus.Text = "SQLite GUI: Changes Updated";
                    tmrTimer.Enabled = true;
                }
                else
                {
                    sblblstatus.Text = "SQLite GUI: No changes to update!!";
                    tmrTimer.Enabled = true;
                }
            }
            catch (InvalidOperationException invalidex)
            {
                MessageBox.Show(invalidex.Message + "\nRefer Help -> topics -> Help 1");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        //<summary>
        //About Box
        //</summary>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        //<summary>
        //Displays the help
        //</summary>
        private void toolStripHelp_Click(object sender, EventArgs e)
        {
            Process.Start(Environment.CurrentDirectory + "\\Help\\SQLite_Gui_Help.html");
        }

        //<summary>
        //Displays the schema for the available tables in the DB
        //</summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();

                if (DB_NAME == null)
                {
                    MessageBox.Show("Open an existing database or Create a new database", "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string connString = String.Format("Data Source={0};New=False;Version=3", DB_NAME);

                SQLiteConnection sqlconn = new SQLiteConnection(connString);

                sqlconn.Open();

                string CommandText = "SELECT * FROM sqlite_master;";

                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(CommandText, sqlconn);
                dataAdapter.Fill(ds);

                DataRowCollection dataRowCol = ds.Tables[0].Rows;

                txtSchema.Clear();

                foreach (DataRow dr in dataRowCol)
                {
                    txtSchema.Text += "\n________________________________________________\n";

                    txtSchema.Text += "\nType :";
                    txtSchema.Text += dr["type"];

                    txtSchema.Text += "\nTable Name :";
                    txtSchema.Text += dr["name"];

                    txtSchema.Text += "\nSQL :";
                    txtSchema.Text += dr["sql"];

                    txtSchema.Text += "\n________________________________________________\n";

                }

                sqlconn.Close();
            }
            catch (SQLiteException sqliteex)
            {
                MessageBox.Show(sqliteex.Message);
            }
        }

        //<summary>
        //Connect to the existing DB
        //</summary>
        private void openDatabase()
        {
            try
            {
                DataSet ds = new DataSet();
                opnfiledlg.CheckPathExists = true;
                opnfiledlg.CheckFileExists = true;
                opnfiledlg.Filter = "DataBase Files (*.db)|*.db|All Files (*.*)|*.*";
                opnfiledlg.Multiselect = false;
                opnfiledlg.Title = "Select SQLite Database File";

                if (opnfiledlg.ShowDialog() == DialogResult.OK)
                {
                    //Connects To SQLiteDatabase

                    lblconnected.Text = "YES";

                    DB_NAME = opnfiledlg.FileName;
                    lblDatabase.Text = DB_NAME;
                    reload_tables();
                }
            }
            catch (SQLiteException sqliteex)
            {
                MessageBox.Show(sqliteex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //<summary>
        //Create and connect to the new DB
        //</summary>
        private void newDatabase()
        {
            try
            {
                DataSet ds = new DataSet();
                savefileDlg.Filter = "DB Files(*.db)|*.db|All Files (*.*)|*.*";
                savefileDlg.DefaultExt = ".db";
                savefileDlg.AddExtension = true;

                if (savefileDlg.ShowDialog() == DialogResult.OK)
                {
                    DB_NAME = savefileDlg.FileName;

                    string connString = String.Format("Data Source={0};New=True;Version=3", DB_NAME);

                    SQLiteConnection sqlconn = new SQLiteConnection(connString);

                    sqlconn.Open();

                    lblconnected.Text = "YES";

                    sqlconn.Close();
                }
            }
            catch (SQLiteException sqlex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + sqlex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        //<summary>
        //Prints the data in the DataGridView
        //summary>
        private void printTable()
        {
            try
            {
                if (SetupThePrinting())
                {
                    PrintPreviewDialog MyPrintPreviewDialog = new PrintPreviewDialog();
                    MyPrintPreviewDialog.Document = userPrintDocument;
                    MyPrintPreviewDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool SetupThePrinting()
        {
            try
            {
                PrintDialog MyPrintDialog = new PrintDialog();
                MyPrintDialog.AllowCurrentPage = false;
                MyPrintDialog.AllowPrintToFile = false;
                MyPrintDialog.AllowSelection = false;
                MyPrintDialog.AllowSomePages = false;
                MyPrintDialog.PrintToFile = false;
                MyPrintDialog.ShowHelp = false;
                MyPrintDialog.ShowNetwork = false;

                if (MyPrintDialog.ShowDialog() != DialogResult.OK)
                    return false;

                userPrintDocument.DocumentName = "Table Report";
                userPrintDocument.PrinterSettings = MyPrintDialog.PrinterSettings;
                userPrintDocument.DefaultPageSettings = MyPrintDialog.PrinterSettings.DefaultPageSettings;
                userPrintDocument.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);

                userDataGridViewPrinter = new DataGridViewPrinter(userDataGridView, userPrintDocument, false, true, "Table", new Font("Verdana", 10, FontStyle.Bold, GraphicsUnit.Point), Color.Black, true);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to execute your query due to the error below: \r\n" + ex.Message, "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }

        private void closeDatabase()
        {
            DB_NAME = null;
            lblconnected.Text = "NO";
        }
       
        private void refreshTables()
        {
            if (DB_NAME == null)
            {
                MessageBox.Show("Open an existing database or Create a new database", "SQLite GUI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            reload_tables();
        }

        private void userPrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            bool more = userDataGridViewPrinter.DrawDataGridView(e.Graphics);
            if (more == true)
                e.HasMorePages = true;
        }

        private void tlStpbtnnew_Click(object sender, EventArgs e)
        {
            newDatabase();
        }

        private void newDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newDatabase();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openDatabase();
        }

        private void tlstrpbtnconnect_Click(object sender, EventArgs e)
        {
            openDatabase();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printTable();
        }

        private void tlstrpprint_Click(object sender, EventArgs e)
        {
            printTable();
        }

        private void tlStrpClose_Click(object sender, EventArgs e)
        {
            closeDatabase();
        }

        private void closeDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeDatabase();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tmrTimer_Tick(object sender, EventArgs e)
        {
            sblblstatus.Text = "Ready";
            tmrTimer.Enabled = false;
        }

        private void selectStripMenuItem_Click(object sender, EventArgs e)
        {
            txtCustomQuery.SelectAll();
        }

        private void delStripMenuItem_Click(object sender, EventArgs e)
        {
            txtCustomQuery.Clear();
        }

        private void pasteStripMenuItem_Click(object sender, EventArgs e)
        {
            txtCustomQuery.Paste();
        }

        private void cutStripMenuItem_Click(object sender, EventArgs e)
        {
            txtCustomQuery.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtCustomQuery.Copy();
        }

        private void chkWordWrap_CheckedChanged(object sender, EventArgs e)
        {
            if (chkWordWrap.Checked)
            {
                txtCustomQuery.WordWrap = false;
                chkWordWrap.Checked = false;
            }
            else
            {
                txtCustomQuery.WordWrap = true;
                chkWordWrap.Checked = true;
            }
        }

        private void userDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

    }
}