package com.example.a11720497.hentaiudpsender;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;

import java.io.IOException;

public class MainActivity extends AppCompatActivity implements UDPSocket.OnDataReceivedListener {

    //Depois fazer um Joystick para mandar grau para a placa

    private UDPSocket socket;
    private Button buttonW;
    private Button buttonS;
    private Button buttonA;
    private Button buttonD;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        buttonW = (Button)findViewById(R.id.buttonW);
        buttonS = (Button)findViewById(R.id.buttonS);
        buttonA = (Button)findViewById(R.id.buttonA);
        buttonD = (Button)findViewById(R.id.buttonD);

        buttonW.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                if(event.getActionMasked() == MotionEvent.ACTION_DOWN) {
                    enviarComando("W");
                }
                if(event.getActionMasked() == MotionEvent.ACTION_UP) {
                    enviarComando("w");
                }
                return false;
            }
        });
        buttonS.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                if(event.getActionMasked() == MotionEvent.ACTION_DOWN) {
                    enviarComando("S");
                }
                if(event.getActionMasked() == MotionEvent.ACTION_UP) {
                    enviarComando("s");
                }
                return false;
            }
        });
        buttonA.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                if(event.getActionMasked() == MotionEvent.ACTION_DOWN) {
                    enviarComando("A");
                }
                if(event.getActionMasked() == MotionEvent.ACTION_UP) {
                    enviarComando("a");
                }
                return false;
            }
        });
        buttonD.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                if(event.getActionMasked() == MotionEvent.ACTION_DOWN) {
                    enviarComando("D");
                }
                if(event.getActionMasked() == MotionEvent.ACTION_UP) {
                    enviarComando("d");
                }
                return false;
            }
        });
    }

    @Override
    protected void onStart() {
        super.onStart();

        try {
            socket = new UDPSocket(this, 6200);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    @Override
    protected void onStop() {
        super.onStop();

        socket.close();
    }

    public void enviarComando(String comando) {
        try {
            socket.send(comando.getBytes(), "192.168.4.1", 6200);
        } catch(IOException e) {
            e.printStackTrace();
        }
    }

    @Override
    public void onDataReceived(byte[] buffer, int offset, int length, String ip, int port) {

    }
}
