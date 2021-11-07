﻿using LHBeverage.Model;
using LHBeverage.ModelService;
using LHBeverage.UserControls.Component;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace LHBeverage.UserControls
{
    public partial class CartPagePanel : UserControl
    {
        Cart cartinfo;
        CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
        public CartPagePanel(Cart cart)
        {
            InitializeComponent();
            CreateCartItem(cart);
        }
        private void CreateCartItem(Cart cart)
        {
            int Subtotal=0;
            int Total;
            int ShippingPrice = 30000;
            cartinfo = cart;
            List<DetailCart> itemcarts = DetailCartConnect.LoadDetailCart(cart);
            foreach (DetailCart itemcart in itemcarts)
            {
                string[] idingredients = itemcart.ListIDIngredient.Split(',');
                ItemCart itemCart = new ItemCart(itemcart);
                itemCart.Click += Redisplay;
                ItemsCart.Controls.Add(itemCart);
                Subtotal += itemcart.Price;
            }
            Total = Subtotal + ShippingPrice;
            SubtotalPriceLabel.Text = Subtotal.ToString("#,###", cul.NumberFormat) + " VNĐ";
            ShippingPriceLabel.Text = ShippingPrice.ToString("#,###", cul.NumberFormat) + " VNĐ";
            TotalPriceLabel.Text = Total.ToString("#,###", cul.NumberFormat) + " VNĐ";
            ItemsCart.AutoScroll = true;
        }
        private void Redisplay(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if(btn!=null)
            {
                ItemsCart.Controls.Clear();
                CreateCartItem(cartinfo);
            }
        }

        private void ProceedBtn_Click(object sender, EventArgs e)
        {
            OrderConnect.CreateOrder(cartinfo);
            DetailCartConnect.ClearCart(cartinfo);
            ItemsCart.Controls.Clear();
            CreateCartItem(cartinfo);
        }
    }
}
