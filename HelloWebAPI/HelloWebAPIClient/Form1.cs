using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HelloWebAPIClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Client = new HttpClient() 
            { BaseAddress = new Uri(textBoxURI.Text) };
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void buttonListAllProducts_Click(object sender, EventArgs e)
        {
            listBoxResults.Items.Clear();
            HttpResponseMessage response = Client.GetAsync("api/Products").Result;
            if (response.IsSuccessStatusCode)
            {
                var productsAsync = response.Content.ReadAsAsync<IEnumerable<Product>>();
                var products = productsAsync.Result;
                foreach (var item in products)
                {
                    listBoxResults.Items.Add(item.Id + "\t" + item.Name + "\t" + item.Price + "\t" + item.Category);
                }
            }
            else
            {
                listBoxResults.Items.Add(((int)response.StatusCode).ToString() + ",\t" + response.ReasonPhrase);
            }
        }

        private void button2Params_Click(object sender, EventArgs e)
        {
            listBoxResults.Items.Clear();
            HttpResponseMessage response = Client.GetAsync("api/Products?id=234&str=blablabla").Result;
            if (response.IsSuccessStatusCode)
            {
                var productsAsync = response.Content.ReadAsAsync<Product>();
                var products = productsAsync.Result;
                listBoxResults.Items.Add(products.Id + "\t" + products.Name + "\t" + products.Price + "\t" + products.Category);
                listBoxResults.Items.Add(products.Element.Num + "\t" + products.Element.ElementName);
            }
            else
            {
                listBoxResults.Items.Add(((int)response.StatusCode).ToString() + ",\t" + response.ReasonPhrase);
            }
        }

        private void button2Ints_Click(object sender, EventArgs e)
        {
            listBoxResults.Items.Clear();
            HttpResponseMessage response = Client.GetAsync("api/Products/234?id2=123").Result;
            if (response.IsSuccessStatusCode)
            {
                var productsAsync = response.Content.ReadAsAsync<Product>();
                var products = productsAsync.Result;
                listBoxResults.Items.Add(products.Id + "\t" + products.Name + "\t" + products.Price + "\t" + products.Category);
                listBoxResults.Items.Add(products.Element.Num + "\t" + products.Element.ElementName);
            }
            else
            {
                listBoxResults.Items.Add(((int)response.StatusCode).ToString() + ",\t" + response.ReasonPhrase);
            }
        }

        public HttpClient Client { get; private set; }
    }
}
