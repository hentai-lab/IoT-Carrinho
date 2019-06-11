using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ControleKinect {
	class Program {
		static void Main(string[] args) {
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.Bind(new IPEndPoint(IPAddress.Any, 42001));
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.4.1"), 6200);

			byte[] bufferEnvio = new byte[1];

			byte[] buffer = new byte[1000];
			bool presente = false, maosAoAlto = false, W = false, A = false, S = false, D = false;
			int direcao = 0, aceleracao = 0, bytesEnviados = 0;
			double centroQuadrilX1 = 0, centroQuadrilY1 = 0, centroOmbroX1 = 0, centroOmbroY1 = 0,
				maoEsqX1 = 0, maoEsqY1 = 0, maoDirX1 = 0, maoDirY1 = 0, joelhoEsqY1 = 0, joelhoDirY1 = 0;

			StringBuilder builder = new StringBuilder(1000);

			while (true) {
				try {
					int tamanho = socket.Receive(buffer);
					if (tamanho < 26)
						continue;

					string[] partes = Encoding.ASCII.GetString(buffer, 18, tamanho - 18).Split(' ');
					if (partes.Length % 2 != 0)
						continue;

					for (int i = 0; i < partes.Length; i += 2) {
						switch (partes[i]) {
							case "\"Presente1\"":
								presente = (partes[i + 1] == "1");
								break;
							case "\"CentroQuadrilX1\"":
								double.TryParse(partes[i + 1], out centroQuadrilX1);
								break;
							case "\"CentroQuadrilY1\"":
								double.TryParse(partes[i + 1], out centroQuadrilY1);
								break;
							case "\"CentroOmbroX1\"":
								double.TryParse(partes[i + 1], out centroOmbroX1);
								break;
							case "\"CentroOmbroY1\"":
								double.TryParse(partes[i + 1], out centroOmbroY1);
								break;
							case "\"MaoEsqX1\"":
								double.TryParse(partes[i + 1], out maoEsqX1);
								break;
							case "\"MaoEsqY1\"":
								double.TryParse(partes[i + 1], out maoEsqY1);
								break;
							case "\"MaoDirX1\"":
								double.TryParse(partes[i + 1], out maoDirX1);
								break;
							case "\"MaoDirY1\"":
								double.TryParse(partes[i + 1], out maoDirY1);
								break;
							case "\"JoelhoEsqY1\"":
								double.TryParse(partes[i + 1], out joelhoEsqY1);
								break;
							case "\"JoelhoDirY1\"":
								double.TryParse(partes[i + 1], out joelhoDirY1);
								break;
						}
					}

					double comprimentoColuna;
					double angulo;

					if (presente) {
						double dx = centroOmbroX1 - centroQuadrilX1;
						double dy = centroOmbroY1 - centroQuadrilY1;
						comprimentoColuna = Math.Sqrt((dx * dx) + (dy * dy));

						double coluna2 = comprimentoColuna * 0.5;
						double coluna4 = comprimentoColuna * 0.25;
						double coluna8 = comprimentoColuna * 0.125;

						double yMinimoParaMaosAoAlto = centroQuadrilY1 + coluna2;
						double yMinimoParaVoltarMaosParaBaixo = centroQuadrilY1 + coluna4;

						if (maoDirY1 >= yMinimoParaMaosAoAlto && maoEsqY1 >= yMinimoParaMaosAoAlto)
							maosAoAlto = true;
						else if (maoDirY1 < yMinimoParaVoltarMaosParaBaixo && maoEsqY1 < yMinimoParaVoltarMaosParaBaixo)
							maosAoAlto = false;

						if (maosAoAlto) {
							angulo = (180 / Math.PI) * Math.Atan2(maoDirY1 - maoEsqY1, maoDirX1 - maoEsqX1);
							// angulo > 0 = virando para esquerda (mão direita acima da esquerda)
							// angulo < 0 = virando para direita (mão direita abaixo da esquerda)
							if (angulo > 0) {
								if (angulo > 30)
									direcao = 1;
								else if (angulo < 15)
									direcao = 0;
							} else {
								if (angulo < -30)
									direcao = -1;
								else if (angulo > -15)
									direcao = 0;
							}

							double joelhoDelta = joelhoEsqY1 - joelhoDirY1;

							if (joelhoDelta > 0) {
								if (joelhoDelta > coluna4)
									aceleracao = 1;
								else if (joelhoDelta < coluna8)
									aceleracao = 0;
							} else {
								if (joelhoDelta < -coluna4)
									aceleracao = -1;
								else if (joelhoDelta > -coluna8)
									aceleracao = 0;
							}

						} else {
							angulo = 0;
							direcao = 0;
							aceleracao = 0;
						}
					} else {
						comprimentoColuna = 0;
						maosAoAlto = false;
						angulo = 0;
						direcao = 0;
						aceleracao = 0;
					}

					builder.Remove(0, builder.Length);
					builder.Append(presente ? "P " : "- ");
					builder.Append(maosAoAlto ? "MAO " : "    ");
					builder.Append(direcao == 0 ? "   " : (direcao > 0 ? "ESQ" : "DIR"));
					builder.Append(aceleracao < 0 ? " FRE" : "    ");
					builder.Append(" Coluna ");
					builder.Append(comprimentoColuna.ToString("0.00"));
					builder.Append(" Angulo ");
					builder.Append(angulo.ToString("0.00"));
					builder.Append(" E ");
					builder.Append(joelhoEsqY1.ToString("0.00"));
					builder.Append(" D ");
					builder.Append(joelhoDirY1.ToString("0.00"));

					bool w = false, a = false, s = false, d = false;
					if (maosAoAlto) {
						if (aceleracao < 0)
							s = true;
						else
							w = true;
						if (direcao > 0)
							a = true;
						else if (direcao < 0)
							d = true;
					}

					if (!w && W) {
						W = false;
						bufferEnvio[0] = (byte)'w';
						socket.SendTo(bufferEnvio, endPoint);
						bytesEnviados++;
					}
					if (!a && A) {
						A = false;
						bufferEnvio[0] = (byte)'a';
						socket.SendTo(bufferEnvio, endPoint);
						bytesEnviados++;
					}
					if (!s && S) {
						S = false;
						bufferEnvio[0] = (byte)'s';
						socket.SendTo(bufferEnvio, endPoint);
						bytesEnviados++;
					}
					if (!d && D) {
						D = false;
						bufferEnvio[0] = (byte)'d';
						socket.SendTo(bufferEnvio, endPoint);
						bytesEnviados++;
					}

					if (w && !W) {
						W = true;
						bufferEnvio[0] = (byte)'W';
						socket.SendTo(bufferEnvio, endPoint);
						bytesEnviados++;
					}
					if (a && !A) {
						A = true;
						bufferEnvio[0] = (byte)'A';
						socket.SendTo(bufferEnvio, endPoint);
						bytesEnviados++;
					}
					if (s && !S) {
						S = true;
						bufferEnvio[0] = (byte)'S';
						socket.SendTo(bufferEnvio, endPoint);
						bytesEnviados++;
					}
					if (d && !D) {
						D = true;
						bufferEnvio[0] = (byte)'D';
						socket.SendTo(bufferEnvio, endPoint);
						bytesEnviados++;
					}

					Console.Clear();
					Console.WriteLine(bytesEnviados);
					Console.Write(builder.ToString());

				} catch {
					// Apenas ignora...
				}


			}
		}
	}
}

