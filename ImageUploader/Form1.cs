using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Drawing.Imaging;

namespace ImageUploader
{
    public partial class Form1 : Form
    {
        byte[] data = null;
        SqlConnection conn = null;
        SqlCommand selectcmd = null;
        SqlDataAdapter sde = null;
        DataSet ds = null;

        public Form1()
        {
            InitializeComponent();
           
          



            //int id;
                try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["ShoppingDb"].ConnectionString;
                conn = new SqlConnection(connectionString);
                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();
                }

                //color
                selectcmd = new SqlCommand("Select ColorId,ColorName from ColorMaster order by ColorName asc", conn);
                sde  = new SqlDataAdapter(selectcmd);
                ds = new DataSet();
                sde.Fill(ds);
                cmbColor.DataSource = ds.Tables[0];
                cmbColor.DisplayMember = "ColorName";
                cmbColor.ValueMember = "ColorId";

                ds.Tables.Clear(); //remove tables

                selectcmd = new SqlCommand("Select SizeId,SizeName from SizeMaster order by SizeName asc", conn);
                sde = new SqlDataAdapter(selectcmd);
                ds = new DataSet();
                sde.Fill(ds);
                cmbSize.DataSource = ds.Tables[0];
                cmbSize.DisplayMember = "SizeName";
                cmbSize.ValueMember = "SizeId";


                ds.Tables.Clear(); //remove tables

                selectcmd = new SqlCommand("Select SupplierId,SupFirstName from Suppliers order by SupFirstName asc", conn);
                sde = new SqlDataAdapter(selectcmd);
                ds = new DataSet();
                sde.Fill(ds);
                cmbSupId.DataSource = ds.Tables[0];
                cmbSupId.DisplayMember = "SupFirstName";
                cmbSupId.ValueMember = "SupplierId";

                ds.Tables.Clear(); //remove tables

                selectcmd = new SqlCommand("Select id,MenuName from Menu where parentid=0 order by MenuName asc", conn);
                sde = new SqlDataAdapter(selectcmd);
                ds = new DataSet();
                sde.Fill(ds);
                cmbMenu.DataSource = ds.Tables[0];
                cmbMenu.DisplayMember = "MenuName";
                cmbMenu.ValueMember = "id";
                cmbMenu.SelectedIndex = 1;





            }

            catch (Exception e)
            {


            }

            finally
            {
                if (conn.State.Equals(ConnectionState.Open))
                {
                    conn.Close();
                }
            }




        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            int size = -1;

          
            OpenFile.InitialDirectory = "d:\\";
            OpenFile.Filter = "Image Files(*.BMP; *.JPG; *.GIF)| *.BMP; *.JPG; *.GIF | All files(*.*) | *.* ";    //"txt files (*.txt)|*.txt|All files (*.*)|*.*";
            DialogResult result = OpenFile.ShowDialog(); 

            if (result == DialogResult.OK) 
            {

                try
                {
                   
                    txtFile.Text = OpenFile.FileName;
                    // First load the image somehow
                    Image myImage = Image.FromFile(txtFile.Text.Trim(), true);
                    // Save the image with a quality of 50% 

                    string path = AppDomain.CurrentDomain.BaseDirectory;
                    SaveJpeg(path+"temp.jpg", myImage, 50);



                    data = ReadAllBytes(path + "temp.jpg");
                 

                    ////StoreImage(data, txtFile.Text.Trim());
                }
                catch (IOException)
                {
                }
            }
        }

     
        private byte[] ReadAllBytes(string fileName)
        {
            byte[] buffer = null;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
                fs.Close();
            }
            return buffer;
        }


        private void StoreImage(byte[] content)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ShoppingDb"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            if (conn.State.Equals(ConnectionState.Closed))
                conn.Open();
            try
            {

                string insertQuery= "Insert into ItemMaster (SizeId,ColorId,Image1,SupplierId,ItemName," +
                    "ItemDescripton," +
                    "Price,InitialQty,OfferPrice,Brand,ChildMenuId,DeliveryCharges,AvailableQty)" +
                    " values (@sizeId,@colorId,@image,@supId," +
                    "@itemName,@itemDesc,@price,@iniQty,@offerPrice,@Brand,@childMenuId,@delCharge,@AvailableQty)";


                SqlCommand insert = new SqlCommand(insertQuery, conn);

                DataRow selectedSizeDataRow = ((DataRowView)cmbSize.SelectedItem).Row;
                int sizeid = Convert.ToInt32(selectedSizeDataRow["SizeId"]);
                SqlParameter sizePara = insert.Parameters.Add("@sizeId", SqlDbType.Int);
                sizePara.Value = sizeid;


                DataRow selectedDataRow = ((DataRowView)cmbColor.SelectedItem).Row;
                int colorid = Convert.ToInt32(selectedDataRow["ColorId"]);

                SqlParameter colorIdPara = insert.Parameters.Add("@colorId", SqlDbType.Int);
                colorIdPara.Value = colorid;

                SqlParameter imageParameter = insert.Parameters.Add("@image", SqlDbType.Binary);
                imageParameter.Value = content;
                imageParameter.Size = content.Length;


                DataRow selectedSupDataRow = ((DataRowView)cmbSupId.SelectedItem).Row;
                int supId = Convert.ToInt32(selectedSupDataRow["SupplierId"]);

                SqlParameter supPara = insert.Parameters.Add("@supId", SqlDbType.Int);
                supPara.Value = supId;


                SqlParameter itemNamePara = insert.Parameters.Add("@itemName", SqlDbType.NVarChar);
                itemNamePara.Value = txtItemName.Text.Trim();

                SqlParameter itemDescPara = insert.Parameters.Add("@itemDesc", SqlDbType.NVarChar);
                itemDescPara.Value = txtItemDesc.Text.Trim();

                SqlParameter pricePara = insert.Parameters.Add("@price", SqlDbType.Int);
                pricePara.Value = Convert.ToDouble(txtPrice.Text.Trim());

                SqlParameter iniQtyPra = insert.Parameters.Add("@iniQty", SqlDbType.Int);
                iniQtyPra.Value = Convert.ToDouble(txtInitialQty.Text);

                SqlParameter offerPricePara = insert.Parameters.Add("@offerPrice", SqlDbType.Int);
                offerPricePara.Value = Convert.ToDouble(txtOfferPrice.Text.Trim());

                SqlParameter BrandPara = insert.Parameters.Add("@Brand", SqlDbType.NVarChar);
                BrandPara.Value = txtBrand.Text.Trim();


                DataRow selectedSubMenuDataRow = ((DataRowView)cmbSubMenu.SelectedItem).Row;
                int parentid = Convert.ToInt32(selectedSubMenuDataRow["id"]);
              
                SqlParameter childMenuIdPara = insert.Parameters.Add("@childMenuId", SqlDbType.Int);
                childMenuIdPara.Value = parentid;


                SqlParameter delChargePara = insert.Parameters.Add("@delCharge", SqlDbType.Int);
                delChargePara.Value=Convert.ToDouble(txtDelCharge.Text.Trim());

                SqlParameter AvailableQtypara = insert.Parameters.Add("@AvailableQty", SqlDbType.Int);
                AvailableQtypara.Value = Convert.ToDouble(txtInitialQty.Text.Trim());

                insert.ExecuteNonQuery();
                //  MessageBox.Show("Image has been added successfully"); success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                MessageBox.Show(ex.StackTrace.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtFile.Text.Trim() == "" || txtItemName.Text.Trim() == "" || txtItemDesc.Text.Trim() == "" || txtPrice.Text.Trim() == "" || txtInitialQty.Text == "" || txtOfferPrice.Text.Trim() == "" || txtBrand.Text.Trim() == "" || txtDelCharge.Text.Trim() == "")
            {
                lblError.Visible = true;
                lblError.Text = "Enter all required fields";
                return;
            }
            else
            { lblError.Visible = false; }


            StoreImage(data);
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(path + "temp.jpg"))
            {
                File.Delete(path + "temp.jpg");
            }
        }


        /// <summary> 
        /// Saves an image as a jpeg image, with the given quality 
        /// </summary> 
        /// <param name="path"> Path to which the image would be saved. </param> 
        /// <param name="quality"> An integer from 0 to 100, with 100 being the highest quality. </param> 
        public static void SaveJpeg(string path, Image img, long quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

            // Encoder parameter for image quality 
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            // JPEG image codec 
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;
            img.Save(path, jpegCodec, encoderParams);
        }

        /// <summary> 
        /// Returns the image codec with the given mime type 
        /// </summary> 
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }

        private void cmbMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRow selectedDataRow = ((DataRowView)cmbMenu.SelectedItem).Row;
            int parentid = Convert.ToInt32(selectedDataRow["id"]);
           string MenuName = selectedDataRow["MenuName"].ToString();


            var connectionString = ConfigurationManager.ConnectionStrings["ShoppingDb"].ConnectionString;
            conn = new SqlConnection(connectionString);
            if (conn.State.Equals(ConnectionState.Closed))
            {
                conn.Open();
            }

            selectcmd = new SqlCommand("Select id,MenuName from Menu where parentid= " + parentid + " order by MenuName asc", conn);
            sde = new SqlDataAdapter(selectcmd);
            ds = new DataSet();
            sde.Fill(ds);
            cmbSubMenu.DataSource = ds.Tables[0];
            cmbSubMenu.DisplayMember = "MenuName";
            cmbSubMenu.ValueMember = "id";
            ds.Tables.Clear();
        }
    }
}

