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

    def get_account(self):
        return self._send_data("GetAccount")

    def get_friends(self):
        return self._send_data("GetFriends")

    def get_audio_bots(self):
        return self._send_data("GetAudioBots")

    def active_audio_bot(self, bot):
        self._send_data("ActiveAudioBots;" + bot)

    def deactive_audio_bots(self, bot):
        self._send_data(("DeactiveAudioBots;" + bot))

    def get_volume(self):
        return self._send_data("GetVolume")


if __name__ == '__main__':
    # for testing
    client = SocketClient()
    print client.get_account()
    friends = client.get_friends()
    friends_list = friends.split(";")
    for friend in friends_list:
        print friend
    audio_list = client.get_audio_bots()
    for audio in audio_list.split(";"):
        print audio
    client.active_audio_bot("daniel_local")
    print client.get_volume()

