
const char IPD0[] = "+IPD,0,";

void limparSerial() {
  while (Serial.available()) {
    Serial.read();
  }
}

void piscar(byte pino, byte vezes) {
  for (byte i = 0; i < vezes; i++) {
    digitalWrite(pino, 1);
    delay(50);
    digitalWrite(pino, 0);
    delay(50);
  }
}

void travar() {
  for (;;) {
    piscar(PinoR, 2);
  }
}

boolean enviarComando(const char* comando, int tempoDeEsperaTotal, const char* sucesso, byte pinoEspera) {
  if (pinoEspera) {
    digitalWrite(pinoEspera, 1);
  }
  Serial.print(comando);
  byte tamanho = strlen(sucesso);
  char temp[tamanho];
  long tempoLimite = millis() + tempoDeEsperaTotal;
  boolean ok = false;
  while (tempoLimite > millis()) {
    while (Serial.available()) {
      const char c = Serial.read();
      memmove(temp, temp + 1, tamanho - 1);
      temp[tamanho - 1] = c;
      if (!memcmp(temp, sucesso, tamanho)) {
        ok = true;
        goto _FIM;
      }
    }
  }
_FIM:;
  if (pinoEspera) {
    digitalWrite(pinoEspera, 0);
  }
  return ok;
}

int receberDados(int tempoDeEsperaTotal, byte pinoEspera) {
  long tempoLimite = millis() + tempoDeEsperaTotal;
  char c = 0;
  while (Serial.available() && tempoLimite > millis()) {
    c = Serial.read();
    if (c == '+') {
      break;
    }
  }
  if (c != '+') {
    return 0;
  }
  if (pinoEspera) {
    digitalWrite(pinoEspera, 1);
  }
  byte tamanho = 1; // Pula o + do "+IPD,0,", que já foi recebido
  boolean procurandoTamanhoRetorno = false;
  int tamanhoRetorno = 0;
  while (tempoLimite > millis()) {
    while (Serial.available()) {
      c = Serial.read();
      if (!procurandoTamanhoRetorno) {
        if (IPD0[tamanho] != c) {
          // Não era o que estávamos esperando
          goto _FIM;
        }
        tamanho++;
        if (tamanho >= 7) { // "+IPD,0,"
          // Agora descobre quantos bytes foram enviados
          procurandoTamanhoRetorno = true;
          tamanhoRetorno = 0;
        }
      } else {
        if (c == ':') {
          goto _FIM;
        } else if (c >= '0' && c <= '9') {
          tamanhoRetorno = (tamanhoRetorno * 10) + (c - '0');
        } else {
          tamanhoRetorno = 0;
          goto _FIM;
        }
      }
    }
  }
  tamanhoRetorno = 0;
_FIM:;
  if (pinoEspera) {
    digitalWrite(pinoEspera, 0);
  }
  return tamanhoRetorno;
}

