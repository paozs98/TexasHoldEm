﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace WPFpruebaCliente
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        
        string serverIp = "localhost";
        int port = 8080;
        TcpClient clientSocket;

        public Login()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //clientSocket.Close();
            Close();    
        }

        public string readDelServidor() {
            return null;
        }
        

        private void Ingresar_Click(object sender, RoutedEventArgs e)
        {
            
             clientSocket = new TcpClient(serverIp, port); // hace la conexion de una vez 
            //esto se manda al Accept del server;

            Jugador j = new Jugador() { nombre = usuario.Text, contrasena = password.Text}; // agarrando los datos de form

            string jugadorJSON = JsonConvert.SerializeObject(j);

            //metodito papu 
            byte[] flujoBytes = Encoding.Default.GetBytes(jugadorJSON);

            NetworkStream stream = clientSocket.GetStream();

            stream.Write(flujoBytes, 0, flujoBytes.Length);
            //cierre de metodo papu como en una clase 


            //stream.Close();
            

        }

        private void Registrar_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
