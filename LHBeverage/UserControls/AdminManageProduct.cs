﻿using LHBeverage.Model;
using LHBeverage.ModelService;
using LHBeverage.UserControls.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LHBeverage.UserControls
{
    public partial class AdminManageProduct : UserControl
    {
        public static AdminManageProduct instance;
        List<Category> categories;
        List<Product> Products;
        Customer cust;
        Bitmap bmp;
        string base64Img;
        Dictionary<string, string> comboSource = new Dictionary<string, string>();
        Product EditProduct = new Product();
        public AdminManageProduct(Customer customer)
        {
            InitializeComponent();
            loadCategoryInComboBox();
            loadData();
            instance = this;
            cust = customer;
            
        }
        private void loadCategoryInComboBox()
        {
            Dictionary<string, string> comboSourceManageCate = new Dictionary<string, string>();
            categories = CategoryConnect.LoadCategory();
            comboSourceManageCate.Add("0", "All");
            // create Dictionnary comboSource to 
            foreach (Category category in categories)
            {
                comboSource.Add(category.IDCate.ToString(), category.Name);
                comboSourceManageCate.Add(category.IDCate.ToString(), category.Name);
            }
            category_Cb.DataSource = new BindingSource(comboSource, null);
            category_Cb.DisplayMember = "Value";
            category_Cb.ValueMember = "Key";

            cbEditCategory.DataSource = new BindingSource(comboSource, null);
            cbEditCategory.DisplayMember = "Value";
            cbEditCategory.ValueMember = "Key";

            CategoryManage_Cb.DataSource = new BindingSource(comboSourceManageCate, null);
            CategoryManage_Cb.DisplayMember = "Value";
            CategoryManage_Cb.ValueMember = "Key";
        }
        public void loadData()
        {
            int i = 0;
            int categoryManage = Convert.ToInt32(((KeyValuePair<string, string>)CategoryManage_Cb.SelectedItem).Key);
            if (categoryManage == 0)
            {
                Products = ProductConnect.LoadProduct();
            }
            else
            {
                Products = ProductConnect.SelectProductByCategory(categoryManage);

            }
           
            ListPro_flowpanel.Controls.Clear();
            foreach (Product product in Products)
            {
                i = 1;            
                List<DetailImage> detailImages = DetailImageConnect.LoadImage(product.IDPro);
                if(detailImages.Count == 0)
                {
                    AdminProductCard adminProductCard = new AdminProductCard(null, product);
                    ListPro_flowpanel.Controls.Add(adminProductCard);
                }
                foreach (DetailImage detailImage in detailImages)
                {
                    if(i == 1)
                    {
                        Bitmap bmp_imgPro = converBase64ToBitmap(detailImage.ImageData);
                        AdminProductCard adminProductCard = new AdminProductCard(bmp_imgPro, product);
                        ListPro_flowpanel.Controls.Add(adminProductCard);
                        break;
                    }                   
                }
            }          
        }

        // Function convert image:
        public static string convertBitmapToBase64(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            string base64ImageString = Convert.ToBase64String(byteImage);
            return base64ImageString;
        }
        public static Bitmap converBase64ToBitmap(string base64)
        {
            Bitmap bmpReturn = null;
            //Convert Base64 string to byte[]
            byte[] byteBuffer = Convert.FromBase64String(base64);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;
            bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);
            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;
            return bmpReturn;
        }
        // manage product:
        private void AddPro_btn_Click(object sender, EventArgs e)
        {           
                try
                {
                    int key = Convert.ToInt32(((KeyValuePair<string, string>)category_Cb.SelectedItem).Key);
                    // create product:
                    Product product = new Product();
                    product.IDCate = key;
                    product.IDCust = cust.IDCus;
                    product.Name = NamePro_tb.Text;
                    product.PriceS = Convert.ToInt32(PriceS_tb.Text);
                    product.PriceM = Convert.ToInt32(PriceM_tb.Text);
                    product.PriceL = Convert.ToInt32(PriceL_tb.Text);
                    product.Description = Des_Tb.Text;
                    product.Quantity = Convert.ToInt32(Quantity_tb.Text);
                    product.Type = "Normal";

                    ProductConnect.CreateProduct(product);
                    MessageBox.Show("Add Product successfull");

                    NamePro_tb.Text = "";
                    Des_Tb.Text = "";
                    Quantity_tb.Text = "";
                    PriceS_tb.Text = "";
                    PriceM_tb.Text = "";
                    PriceL_tb.Text = "";
                    AddPro_panel.Visible = false;
                    AddImg_panel.Visible = true;

                    flowPanel_AddImagePro.Controls.Clear();
                    CategoryManage_Cb.SelectedValue = product.IDCate.ToString();
                    renderListProduct();
                    loadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }        
        }
        public void Edit_Product(Product product)
        {
            EditProduct = product;
            EditProductName_lbl.Text = product.Name;
            panelEditCtn.Visible = true;
            panelCtnUpdateInfor.Visible = true;
            panelCtnUpdateInfor.BringToFront();
            panelCtnUpdateImg.Visible = false;
     
            if (product != null)
            {
                EditProductName_tb.Text = product.Name;
                EditPriceS_tb.Text = product.PriceS.ToString();
                EditPriceM_tb.Text = product.PriceM.ToString();
                EditPriceL_tb.Text = product.PriceL.ToString();
                EditDes_tb.Text = product.Description;
                EditQuantity_tb.Text = product.Quantity.ToString();
                // set selecteditem in category of product edit and selected product in upload image
                try
                {
                    cbEditCategory.SelectedValue = product.IDCate.ToString();
                    //List<Category> categories = CategoryConnect.SelectCategory(product.IDCate);                  
                    //foreach (Category category in categories)
                    //{                       
                    //    cbEditCategory.SelectedValue = category.IDCate.ToString();
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void AddImg_btn_Click(object sender, EventArgs e)
        {
            PublicParam.dataTableImgPro.Clear();
            renderListProduct();
            int IdPro = Convert.ToInt32(((KeyValuePair<string, string>)ProductList_cb.SelectedItem).Key);
            try
            {
                List<DetailImage> detailImages = DetailImageConnect.LoadImage(IdPro);
                if (detailImages.Count > 0)
                {
                    AddImgOfProToPanel(IdPro, 1);
                }          
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            AddImg_panel.Visible = true;
            AddImg_panel.BringToFront();
            AddPro_panel.Visible = false;
        }

        private void renderListProduct()
        {
            Dictionary<string, string> comboSource = new Dictionary<string, string>();
            List<Product> products = ProductConnect.LoadProduct();
            foreach (Product product in products)
            {
                comboSource.Add(product.IDPro.ToString(), product.Name);
            }
            ProductList_cb.DataSource = new BindingSource(comboSource, null);
            ProductList_cb.DisplayMember = "Value";
            ProductList_cb.ValueMember = "Key";
        }
        
        private void ImageBack_btn_Click(object sender, EventArgs e)
        {
            panelCtnAddPro.Visible = false;
            PublicParam.dataTableImgPro.Clear();
        }

        private void loadImage_Click(object sender, EventArgs e)
        {
            if(openFileDialog_Img.ShowDialog() == DialogResult.OK)
            {
                bmp = new Bitmap(openFileDialog_Img.FileName);
                base64Img = convertBitmapToBase64(bmp);
              
                int idimg = setIdImageByDatatable();
                int IdPro = Convert.ToInt32(((KeyValuePair<string, string>)ProductList_cb.SelectedItem).Key);
                PublicParam.dataTableImgPro.Rows.Add(IdPro, idimg, base64Img);
                renderImgProByTable();           
            }
        }
        // save image to data when add new product
        private void AddImage_Click(object sender, EventArgs e)
        {
          
            int IdPro = Convert.ToInt32(((KeyValuePair<string, string>)ProductList_cb.SelectedItem).Key);
            DetailImageConnect.DeleteImageOfPro(IdPro);
            DataRow[] dr = PublicParam.dataTableImgPro.Select();
            try
            {
                if (dr.Length > 0)
                {
                    foreach (DataRow dataRow in dr)
                    {
                        DetailImage detailImage = new DetailImage();
                        detailImage.IDPro = Convert.ToInt32(dataRow["IDPro"].ToString());
                        detailImage.IDImage = Convert.ToInt32(dataRow["IDImage"].ToString());
                        detailImage.ImageData = dataRow["ImageData"].ToString();
                        DetailImageConnect.CreateDetailImage(detailImage);
                    }
                    MessageBox.Show("Add image for product successfull");
                }
                loadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ProductList_cb_SelectedValueChanged(object sender, EventArgs e)
        {
            int IdPro = Convert.ToInt32(((KeyValuePair<string, string>)ProductList_cb.SelectedItem).Key);
            try{
                List<DetailImage> detailImages = DetailImageConnect.LoadImage(IdPro);
                if (detailImages.Count > 0)
                {
                    AddImgOfProToPanel(IdPro, 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnOpenpanelAddPro_Click(object sender, EventArgs e)
        {
            panelCtnAddPro.Visible = true;
            panelCtnAddPro.BringToFront();
            AddPro_panel.Visible = true;
            panelEditCtn.Visible = false;

            NamePro_tb.Text = "";
            PriceS_tb.Text = "";
            Des_Tb.Text = "";
            Quantity_tb.Text = "";
        }

        private void btn_Toggle_AddInfor_Click(object sender, EventArgs e)
        {
            AddPro_panel.BringToFront();
            AddPro_panel.Visible = true;
            AddImg_panel.Visible = false;
        }

        private void close_panelCtnEdit_Click(object sender, EventArgs e)
        {
            panelEditCtn.Visible = false;
            PublicParam.dataTableImgPro.Clear();
        }

        private void Btn_Nav_EditInfor_Click(object sender, EventArgs e)
        {
            panelCtnUpdateInfor.Visible = true;
            panelCtnUpdateInfor.BringToFront();
            panelCtnUpdateImg.Visible = false;
        }

        // render cardImg từ datatable:
        private void Btn_Nav_UpdateImg_Click(object sender, EventArgs e)
        {
            PublicParam.dataTableImgPro.Clear();
            panelCtnUpdateInfor.Visible = false;
            panelCtnUpdateImg.BringToFront();
            panelCtnUpdateImg.Visible = true;

            AddImgOfProToPanel(EditProduct.IDPro, 0);
         }

        // update vào csdl
        private void EditSubmitImg_btn_Click(object sender, EventArgs e)
        {
            DetailImageConnect.DeleteImageOfPro(EditProduct.IDPro);
            DataRow[] dr = PublicParam.dataTableImgPro.Select();
            try
            {
                if(dr.Length > 0)
                {
                    foreach (DataRow dataRow in dr)
                    {
                        DetailImage detailImage = new DetailImage();
                        detailImage.IDPro = Convert.ToInt32(dataRow["IDPro"].ToString());
                        detailImage.IDImage = Convert.ToInt32(dataRow["IDImage"].ToString());
                        detailImage.ImageData = dataRow["ImageData"].ToString();
                        DetailImageConnect.CreateDetailImage(detailImage);
                    }
                    MessageBox.Show("Update image for product successfull");
                }
               
                loadData();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            } 
        }
        //Update Information of product successfull
        private void UpdateInfo_btn_Click(object sender, EventArgs e)
        {
            // update edit product with infor
            EditProduct.Name = EditProductName_tb.Text;
            EditProduct.PriceS = Convert.ToInt32(EditPriceS_tb.Text);
            EditProduct.PriceM = Convert.ToInt32(EditPriceM_tb.Text);
            EditProduct.PriceL = Convert.ToInt32(EditPriceL_tb.Text);
            EditProduct.Description = EditDes_tb.Text;
            EditProduct.Quantity = Convert.ToInt32(EditQuantity_tb.Text);
            EditProduct.IDCate = Convert.ToInt32(((KeyValuePair<string, string>)cbEditCategory.SelectedItem).Key);
            // update editproduct to database:
            try
            {
                ProductConnect.UpdateProduct(EditProduct);
                MessageBox.Show("Update Information of product successfull");
                CategoryManage_Cb.SelectedValue = EditProduct.IDCate.ToString();
                loadData();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadImgFromLocal_Btn_Click(object sender, EventArgs e)
        {
            if(openFileDialog_Img.ShowDialog() == DialogResult.OK)
            {            
                bmp = new Bitmap(openFileDialog_Img.FileName);
                base64Img = convertBitmapToBase64(bmp);
                int idimg = setIdImageByDatatable();
                PublicParam.dataTableImgPro.Rows.Add(EditProduct.IDPro, idimg, base64Img);
                renderImgEditProByTable();
            }
        }
       
       
        private int setIdImageByDatatable()
        {
            int index = 0;
            index = PublicParam.dataTableImgPro.Rows.Count;
            return index;
        }
       
        void AddImgOfProToPanel(int idpro, int checkAddImg)
        {
            List<DetailImage> detailImages = DetailImageConnect.LoadImage(idpro);
            PublicParam.dataTableImgPro.Clear();
            foreach (DetailImage detailImage in detailImages)
            {
                PublicParam.dataTableImgPro.Rows.Add(idpro, detailImage.IDImage, detailImage.ImageData);
            }
            if(checkAddImg == 0)
            {
                renderImgEditProByTable();
            }
            else
            {
                renderImgProByTable();
            }
        }
        public void renderImgEditProByTable()
        {
            EditImage_FlowPanel.Controls.Clear();
            DataRow[] dr = PublicParam.dataTableImgPro.Select();
            foreach (DataRow dataRow in dr)
            {
                AdminImageCard adminImageCard = new AdminImageCard(converBase64ToBitmap(dataRow["ImageData"].ToString()), Convert.ToInt32(dataRow["IDImage"].ToString()),0);
                EditImage_FlowPanel.Controls.Add(adminImageCard);
            }
        }
      
        public void renderImgProByTable()
        {
            flowPanel_AddImagePro.Controls.Clear();
            DataRow[] dr = PublicParam.dataTableImgPro.Select();
            foreach (DataRow dataRow in dr)
            {
                AdminImageCard adminImageCard = new AdminImageCard(converBase64ToBitmap(dataRow["ImageData"].ToString()), Convert.ToInt32(dataRow["IDImage"].ToString()), 1);
                flowPanel_AddImagePro.Controls.Add(adminImageCard);
            }
        }

        private void CategoryManage_Cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadData();
        }
    }
}
