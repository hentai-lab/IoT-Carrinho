#define pinRight 3
#define pinLeft 9
#define pinForward 11
#define pinBackward 12

#define PinoG 6
#define PinoR 10
#define PinoB 5

const char OK[] = "OK\r\n";
const char READY[] = "ready";
const char ComandoReset[] = "AT+RST\r\n";
const char ComandoModoDeOperacaoAP[] = "AT+CWMODE=2\r\n";
const char ComandoConfigurarConexoes[] = "AT+CIPMUX=1\r\n";
const char ComandoConfigurarAP[] = "AT+CWSAP=\"Carrinho Branco\",\"senha\",1,0\r\n";
const char ComandoCriarConexaoUDP[] = "AT+CIPSTART=0,\"UDP\",\"0.0.0.0\",6200,6200,0\r\n";

void setup() {
  Serial.begin(115200);
  Serial.setTimeout(2000);

  pinMode(pinRight, OUTPUT);
  pinMode(pinLeft, OUTPUT);
  pinMode(pinForward, OUTPUT);
  pinMode(pinBackward, OUTPUT);

  pinMode(PinoG, OUTPUT);
  pinMode(PinoR, OUTPUT);
  pinMode(PinoB, OUTPUT);

  digitalWrite(PinoG, 1);
  delay(5000);
  limparSerial();
  piscar(PinoG, 4);
  
  while (!enviarComando(ComandoReset, 4000, READY, PinoB)) {
    delay(2000);
  }
  piscar(PinoG, 4);

  if (enviarComando(ComandoModoDeOperacaoAP, 2000, OK, PinoB)) {
    piscar(PinoG, 4);
  } else {
    travar();
  }

  if (enviarComando(ComandoConfigurarConexoes, 2000, OK, PinoB)) {
    piscar(PinoG, 4);
  } else {
    travar();
  }

  if (enviarComando(ComandoConfigurarAP, 10000, OK, PinoB)) {
    piscar(PinoG, 4);
  } else {
    travar();
  }

  if (enviarComando(ComandoCriarConexaoUDP, 10000, OK, PinoB)) {
    piscar(PinoG, 4);
  } else {
    travar();
  }

  limparSerial();
}

void loop() {
  int dadosRecebidos = receberDados(5000, PinoB);
  if (!dadosRecebidos) {
    return;
  }

  while (dadosRecebidos > 0) {
    dadosRecebidos--;
    delay(50);
  
    // Pinos definidos
    // pinRight 3
    // pinLeft 9
    // pinForward 11
    // pinBackward 12
    
    char c = Serial.read();
    switch (c) {
      case 'W':
        digitalWrite(pinBackward, 0);
        digitalWrite(pinForward, 1);
        break;
      case 'w':
        digitalWrite(pinBackward, 0);
        digitalWrite(pinForward, 0);
        break;
      case 'S':
        digitalWrite(pinForward, 0);
        digitalWrite(pinBackward, 1);
        break;
      case 's':
        digitalWrite(pinForward, 0);
        digitalWrite(pinBackward, 0);
        break;
      case 'A':
        digitalWrite(pinRight, 0);
        digitalWrite(pinLeft, 1);
        break;
      case 'a':
        digitalWrite(pinRight, 0);
        digitalWrite(pinLeft, 0);
        break;
      case 'D':
        digitalWrite(pinLeft, 0);
        digitalWrite(pinRight, 1);
        break;
      case 'd':
        digitalWrite(pinLeft, 0);
        digitalWrite(pinRight, 0);
        break;
      default:
        piscar(PinoR, 4);
        break;
    }
  }

  //limparSerial();
}

