package com.example.a11720497.hentaiudpsender;

import android.os.Handler;
import android.os.HandlerThread;
import android.os.Message;

import java.io.Closeable;
import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.InetSocketAddress;

@SuppressWarnings({"unused", "WeakerAccess"})
public final class UDPSocket implements Closeable {
    public interface OnDataReceivedListener {
        void onDataReceived(byte[] buffer, int offset, int length, String ip, int port);
    }

    private final Object sendToken, receiveToken;
    private volatile boolean alive;
    private volatile DatagramPacket sentPacket, receivedPacket;
    private Thread thread;
    private OnDataReceivedListener listener;
    private Handler sendHandler, receiveHandler;
    private HandlerThread senderThread;
    private DatagramSocket socket;
    private int port;

    public UDPSocket() throws IOException {
        this(null, 0);
    }

    public UDPSocket(OnDataReceivedListener listener) throws IOException {
        this(listener, 0);
    }

    public UDPSocket(OnDataReceivedListener listener, int port) throws IOException {
        this.listener = listener;
        sendToken = new Object();
        receiveToken = new Object();
        alive = true;
        //socket = (port <= 0 ? new DatagramSocket(new InetSocketAddress(InetAddress.getByName("0.0.0.0"), 0)) : new DatagramSocket(new InetSocketAddress(InetAddress.getByName("0.0.0.0"), port)));
        socket = (port <= 0 ? new DatagramSocket() : new DatagramSocket(port));
        socket.setSoTimeout(0);
        this.port = socket.getLocalPort();
        senderThread = new HandlerThread("UDPSocket Sender Thread");
        senderThread.start();
        sendHandler = new Handler(senderThread.getLooper(), new Handler.Callback() {
            @Override
            public boolean handleMessage(Message msg) {
                synchronized (sendToken) {
                    if (sentPacket == null)
                        return true;
                    try {
                        if (alive)
                            socket.send(sentPacket);
                    } catch (IOException e) {
                        // Se tivéssemos um callback de envio, poderíamos tratar essa exceção
                    }
                    sentPacket = null;
                    sendToken.notifyAll();
                }
                return true;
            }
        });
        if (listener != null) {
            receiveHandler = new Handler(new Handler.Callback() {
                @Override
                public boolean handleMessage(Message msg) {
                    synchronized (receiveToken) {
                        if (receivedPacket == null)
                            return true;
                        if (alive && UDPSocket.this.listener != null)
                            UDPSocket.this.listener.onDataReceived(receivedPacket.getData(), receivedPacket.getOffset(), receivedPacket.getLength(), receivedPacket.getAddress().getHostAddress(), receivedPacket.getPort());
                        receivedPacket = null;
                        receiveToken.notifyAll();
                    }
                    return true;
                }
            });
            thread = new Thread("UDPSocket Receiver Thread") {
                @Override
                public void run() {
                    final byte[] buffer = new byte[32768];
                    final DatagramPacket packet = new DatagramPacket(buffer, buffer.length);
                    while (alive) {
                        try {
                            // Essa técnica não fornece o melhor desempenho, visto que dependemos
                            // do tamanho do buffer do sistema operacional, e apenas um pacote
                            // é recebido/tratado por vez
                            if (socket != null) {
                                packet.setData(buffer);
                                socket.receive(packet);
                                if (packet.getLength() > 0) {
                                    synchronized (receiveToken) {
                                        if (alive) {
                                            UDPSocket.this.receivedPacket = packet;
                                            receiveHandler.sendEmptyMessage(0);
                                            while (UDPSocket.this.receivedPacket != null)
                                                receiveToken.wait();
                                        }
                                    }
                                }
                            }
                        } catch (Exception e) {
                            // Algo estranho aconteceu...
                        }
                    }
                }
            };
            thread.start();
        }
    }

    public int getPort() {
        return port;
    }

    public void send(byte[] buffer, String ip, int port) throws IOException {
        send(buffer, 0, buffer.length, ip, port);
    }

    public void send(byte[] buffer, int offset, int length, String ip, int port) throws IOException {
        if (!alive)
            return;
        // Essa técnica não fornece o melhor desempenho, visto que apenas um pacote
        // pode ser enviado por vez
        final DatagramPacket packet = new DatagramPacket(buffer, offset, length, new InetSocketAddress(InetAddress.getByName(ip), port));
        synchronized (sendToken) {
            if (!alive)
                return;
            try {
                while (sentPacket != null)
                    sendToken.wait();
            } catch (InterruptedException e) {
                // Não há muito o que fazer...
            }
            sentPacket = packet;
            sendHandler.sendEmptyMessage(0);
        }
    }

    @Override
    public void close() {
        alive = false;
        try {
            if (socket != null)
                socket.close();
        } catch (Exception e) {
            // Não há muito o que fazer...
        }
        if (senderThread != null) {
            senderThread.interrupt();
            senderThread.quit();
            senderThread = null;
            sendHandler = null;
        }
        if (thread != null) {
            thread.interrupt();
            try {
                thread.join();
            } catch (InterruptedException e) {
                // Não há muito o que fazer...
            }
            thread = null;
        }
        sentPacket = null;
        receivedPacket = null;
        listener = null;
        receiveHandler = null;
        socket = null;
    }
}
