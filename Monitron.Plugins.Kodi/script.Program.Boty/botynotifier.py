import xbmc
import socket


class SocketClient(object):
    def __init__(self):
        self._ip = 'localhost'
        self._port = 11000
        self._server_address = (self._ip, self._port)

    def _create_new_socket(self):
        # Create a TCP/IP socket
        new_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        # Connect the socket to the port where the server is listening
        new_socket.connect(self._server_address)
        return new_socket

    def _send_data(self, content):
        new_socket = self._create_new_socket()
        data = ""
        try:
            new_socket.sendall(content)
            data += new_socket.recv(2048)
        except Exception:
            data = "None"
        finally:
            new_socket.close()
        return data

    def notify_play_started(self):
        return self._send_data("PlayStarted")


class BotyPlayer(xbmc.Player):
    def __init__(self):
        xbmc.Player.__init__(self)

    def onPlayBackStarted(self):
        xbmc.log("Video Started - Notifying bot")
        socket_client = SocketClient()
        socket_client.notify_play_started()

player = BotyPlayer()
mon = xbmc.Monitor()

while(not mon.waitForAbort(10)):
    pass
