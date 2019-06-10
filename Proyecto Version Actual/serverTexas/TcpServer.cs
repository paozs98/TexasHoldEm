﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace serverTexas
{
    class TcpServer
    {

        IPAddress localAddr = IPAddress.Parse("10.251.34.157");// para que se conecte en cualquir dir
        int puerto = 8090;//cambiar si da problemas con Oracle

        TcpListener ServerSocket;
        TcpClient clientSocket;

        int contadorUsuarios = 0;

        static List<handleClinet> _clients;
        static List<Thread> _hilosClientes;
        int turnoGlobal = 0;

        bool usuarioPermitido;
        Jugador jugador = null;
        Mesa mesa = null;



        public TcpServer()
        {
            ServerSocket = new TcpListener(localAddr, puerto);
            clientSocket = default(TcpClient);
            _hilosClientes = new List<Thread>(); ///iniciando la lista de los clientes 
            _clients = new List<handleClinet>(); //inicializando la lista :p
            this.mesa = new Mesa(); //inicializar la mesa para que el cliente se la mande

        }

        public void IniciarServer()
        {

            try
            {

                ServerSocket.Start();
                Console.WriteLine("Iniciando el server en la direccion {0}", Convert.ToString(localAddr));
                Console.WriteLine("En el puerto {0}", Convert.ToString(puerto));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.Read();
            }

            for (int w = 0; w < 5; w++)
            {// for para obtener las 5 cartas comunes del juego 
             //se muestra solo una por cada ronda 

                Carta carta = mesa.mazoMesa.darUnaCarta();
                mesa.cartasComunes.agregarCarta(carta);

            }
            //provicional para la aceptacion de clientes
            for (int i = 0; i < 4; i++)
            {
                clientSocket = ServerSocket.AcceptTcpClient();
                //jugador = ConvertidorJson.convertirJSONaJugador(this.readData());
                jugador = this.convertirJSONaJugador(this.readData());// esta retornador el jugador
                
                //Aqui se debe crear al handler del cliente 
                Console.WriteLine("Ha entrado un usuario al server! " + jugador.nombre + "\n Jugador numero#" + Convert.ToString(contadorUsuarios));
                this.mesa.jugadores.agregarJugador(new Jugador(jugador.nombre, Convert.ToString(contadorUsuarios), jugador.contrasena));


                contadorUsuarios += 1;
                //this.manejadorCliente(clientSocket, Convert.ToString(contadorUsuarios));

                //handleClinet client = new handleClinet();
                //    _clients.Add(client);
                //    client.iniciarHandleClient(clientSocket, Convert.ToString(contadorUsuarios));
            }



            //for (int i = 0; i < 4; i++)
            //{

            //    clientSocket = ServerSocket.AcceptTcpClient();
            //    jugador = this.convertirJSONaJugador(this.readData());// esta retornador el jugador
            //    usuarioPermitido = TexasHoldemDLL.Autenticación.autentificar(jugador.nombre, jugador.contrasena);
            //    if (usuarioPermitido) //si el usuario existe que entre a la sala para jugar
            //    {
            //        contadorUsuarios += 1;
            //        //Aqui se debe crear al handler del cliente 
            //        Console.WriteLine("Ha entrado un usuario al server! "+jugador.nombre+"\n Jugador numero$" + Convert.ToString(contadorUsuarios));
            //        handleClinet client = new handleClinet();
            //        _clients.Add(client);
            //        client.iniciarHandleClient(clientSocket, Convert.ToString(contadorUsuarios));

            //    }
            //    else // sino mandarle un mensaje de que no existe y que se cree un usuario
            //    {
            //        Console.WriteLine("Usuario no registrado");
            //        this.sendData("Por favor registrese!");

            //    }

            //}

            //aqui se inician los todos los hilos 
            this.letsPlayTexas();

        }


        public Jugador convertirJSONaJugador(string j)
        {

            Jugador juga = new Jugador();
            juga = JsonConvert.DeserializeObject<Jugador>(j);
            return juga;

        }

        public void sendData(String mensaje)
        {// para mansajes al cliente

            string jugadorJSON = JsonConvert.SerializeObject(mensaje);

            byte[] flujoBytes = Encoding.Default.GetBytes(jugadorJSON);

            NetworkStream stream = clientSocket.GetStream();

            stream.Write(flujoBytes, 0, flujoBytes.Length);

        }

        public string readData()
        {

            byte[] receivedBuffer = new byte[4096];// info de lo que envia el cliente pero en byte 
            NetworkStream stream = clientSocket.GetStream();
            stream.Read(receivedBuffer, 0, receivedBuffer.Length);

            StringBuilder msg = new StringBuilder();

            try
            {

                foreach (byte b in receivedBuffer)
                {
                    if (b.Equals(00))
                    {
                        break;
                    }
                    else
                    {
                        msg.Append(Convert.ToChar(b).ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(">>" + ex.ToString());
            }
            return msg.ToString();
        }//cierre del metodo

        public void manejadorCliente(TcpClient inClient, string clientNO)
        {
            TcpClient clienSocket;
            string CLNO;
            Thread clienteHilo;

            clienSocket = inClient;
            CLNO = clientNO;
            clienteHilo = new Thread(letsPlayTexas);
            clienteHilo.Start();
            _hilosClientes.Add(clienteHilo);

        }

        public void letsPlayTexas()
        {


            while (true)
            {
                this.mesa.repartirCartasIniciales();
                this.mesa.pot.apuestaMinima = 50;
                this.mesa.pot.apuestaMaxima = 100;
                this.mesa.jugadores.GetJugadorEnLaPos(1).dineroInicial -= 100;
                this.mesa.jugadores.GetJugadorEnLaPos(2).dineroInicial -= 50;
                foreach (Thread ch in _hilosClientes)
                {
                    ch.Start();
                }

                this.sendData(ConvertidorJson.convertirMesaAJson(this.mesa));

                //Empiece con el jugador 1 
                // Todas las opciones del jugador
                
            }
        }




    }//ciere de la clase 
}
