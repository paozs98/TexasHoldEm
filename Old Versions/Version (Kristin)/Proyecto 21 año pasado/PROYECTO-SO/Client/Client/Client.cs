﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerData;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Client
{
    class Client
    {

        public static Socket master;
        public static string name;
        public static string id;
        public static bool autentificar = false;
        public static string nombreUsuario;
        public static string clave;
        public static bool iniciarJuego = false;//Inicia el juego -- Es un boton
        public static bool abandonarJuego = false;
        public static int carta1;
        public static int carta2;
        public static string seguir;
        public static bool enEspera = false;

        //Variables referencia a la Casa
        public static int cartaCasa1;
        public static int cartaCasa2;
        public static int cartaCasa3;
        public static int cartaCasa4;

        static void Main(string[] args)
        {

            Console.Write("Enter your name: ");
            name = Console.ReadLine();

            A: Console.Clear();
            Console.WriteLine("Enter host IP Address: ");
            string ip = Console.ReadLine();

            master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ips = new IPEndPoint(IPAddress.Parse(ip), 4242);

            try
            {
                master.Connect(ips);
            }
            catch {
                Console.WriteLine("Could not connect to host!");
                Thread.Sleep(1000);
                goto A;
            }

            Thread t = new Thread(DATA_IN);
            t.Start();

            for (; ; )
            {
                string respuesta="";
                string respuestaIniciar="";

                if (autentificar == false)
                {
                    int opcion; //1->Autentificarse 2-> Registrarse
                    Console.WriteLine("Desea autentificarse o registrarse? 1->Autentificarse 2->Registrarse: ");
                    opcion = int.Parse(Console.ReadLine());
                    Packet p;
                    if (opcion == 1)
                    {
                        Console.Write("Digite su nombre de usuario: ");
                        nombreUsuario = Console.ReadLine();
                        Console.Write("Digite su contraseña: ");
                        clave = Console.ReadLine();
                        p = new Packet(Packet.PacketType.autentificar, id);

                    }//Se autentifica
                    else
                    {
                        Console.Write("Digite su nuevo nombre de usuario: ");
                        nombreUsuario = Console.ReadLine();
                        Console.Write("Digite su nueva contraseña: ");
                        clave = Console.ReadLine();
                        p = new Packet(Packet.PacketType.registrar, id);
                    }//Se agrega un nuevo usuario
                    p.Gdata.Add(name);
                    p.nombre = nombreUsuario;
                    p.clave = clave;
                    master.Send(p.ToBytes());
                    autentificar = true;
                }

                else if (autentificar && (!iniciarJuego))//Si está autentificado pero el juego no ha iniciado
                {
                    Console.WriteLine("Desea iniciar el juego?: S=1 / N=0");
                    respuestaIniciar = Console.ReadLine();
                    if (respuestaIniciar.Equals("1")) {
                        Packet p = new Packet(Packet.PacketType.iniciarJuego, id);
                        master.Send(p.ToBytes());//Se envía la petición de iniciar el juego al servidor
                        iniciarJuego = true;
                    }
                }
 
            }
        }


        static void DATA_IN() {
            byte[] Buffer;
            int readBytes;

            for (; ; ) {

                try
                {
                    Buffer = new byte[master.SendBufferSize];
                    readBytes = master.Receive(Buffer);

                    if (readBytes > 0)
                    {
                        DataManager(new Packet(Buffer));
                    }
                }
                catch (SocketException ex) {
                    Console.WriteLine("The server has disconnected!");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
        }

        static void DataManager(Packet p) {

            switch (p.packetType)
            {
                case Packet.PacketType.iniciarJuego:
                    iniciarJuego = true;
                    if (p.nombre.Equals(nombreUsuario)) {
                        carta1 = p.carta1;
                        carta2 = p.carta2;
                        cartaCasa1 = p.cartaCasa1;
                        cartaCasa2 = p.cartaCasa2;
                        Console.WriteLine("Usuario: " + nombreUsuario);
                        Console.WriteLine("C1: " + carta1 + ", C2: " + carta2);
                        Console.WriteLine("Valor de casa: C1: " + cartaCasa1 + ", C2: " + cartaCasa2);
                    }
                    break;
                case Packet.PacketType.Registration:
                    id = p.Gdata[0];
                    break;

                case Packet.PacketType.Chat:
                    ConsoleColor c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(p.Gdata[0] + ": " + p.Gdata[1]);
                    Console.ForegroundColor = c;
                    break;
                case Packet.PacketType.Mensaje:
                    Console.WriteLine(p.nombre + " " + p.id + " " + p.apuesta);
                    break;

                case Packet.PacketType.pedirCarta:
                    //Si el paquete es de tipo pedir carta, el cliente envía su id
                    Console.WriteLine(p.id);
                    break;
                case Packet.PacketType.darCarta:
                    if (p.nombre.Equals(nombreUsuario)) {
                        Console.WriteLine("C3: "+p.numeroCarta);
                    }
                    break;
                case Packet.PacketType.darAcceso:
                    Console.WriteLine("Estado de acceso: " + p.estado);
                    //El cliente se ha autentificado
                    break;
                case Packet.PacketType.denegarAcceso:
                    Console.WriteLine("Estado de acceso: " + p.estado);
                    Environment.Exit(0);
                    //Hace que el programa termine si se le negó el acceso
                    break;
                case Packet.PacketType.comunicarTurno:
                    Console.WriteLine("Llegue a comunicar turno");
                    if (p.turno.Equals(nombreUsuario)) {//Si es el turno del jugador
                        Console.WriteLine("Desea pedir carta o quedarse? (P/Q): ");
                        String r = Console.ReadLine();
                        if (r.Equals("P"))
                        {
                            Console.WriteLine("Pedi una carta");
                            p.packetType = Packet.PacketType.pedirCarta;
                            p.id = clave;//Se envia la clave del cliente
                            p.nombre = nombreUsuario;//Envia el nombre de usuario
                        }
                        else {
                            p.packetType = Packet.PacketType.quedarse;
                            p.id = clave;//Se envia la clave del cliente
                            p.nombre = nombreUsuario;//Envia el nombre de usuario
                        }
                        master.Send(p.ToBytes());
                        Console.WriteLine("Le envie un paquete al server quiero o no");
                    }
                    break;
                case Packet.PacketType.turnoCasa:
                    cartaCasa3 = p.numeroCarta;
                    Console.WriteLine("Carta de casa: " + cartaCasa3);
                    break;
                case Packet.PacketType.casaAcabo:
                    iniciarJuego = false;
                    abandonarJuego = true;
                    if (p.nombre.Equals(nombreUsuario))
                    {
                        Console.WriteLine("El servidor dice que llegamos al final del juego!");
                        Console.WriteLine("Resultado del juego: " + p.estado);
                        Console.WriteLine("Desea seguir jugando? 1:Si 0:No");
                        seguir = Console.ReadLine();
                        if (seguir.Equals("1"))
                        {
                            p.packetType = Packet.PacketType.seguir;
                            p.nombre = nombreUsuario;
                            p.id = clave;
                            master.Send(p.ToBytes());
                        }
                        else
                        {
                            p.packetType = Packet.PacketType.salir;
                            p.nombre = nombreUsuario;
                            p.id = clave;
                            master.Send(p.ToBytes());
                        }
                    }
                    break;
                case Packet.PacketType.enEspera:
                    if (p.clave.Equals(clave))
                    {
                        Console.WriteLine(p.estado);
                        enEspera = true;
                    }
                    break;
            }
        }
    }
}
