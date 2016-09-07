# Import necessary modules
import xbmc
import pyxbmct
import xbmcaddon
import xbmcgui
import os
from SocketClient import SocketClient


_addon = xbmcaddon.Addon()
_path = _addon.getAddonInfo('path')
_check_icon = os.path.join(_path, "check.png")
_person_icon = os.path.join(_path, "person.png")


# Create a class for our UI
class MyAddon(pyxbmct.AddonDialogWindow):

    def __init__(self, title='Boty'):
        """Class constructor"""
        # Call the base class' constructor.
        super(MyAddon, self).__init__(title)
        # socket
        self.socket_client = SocketClient()
        # Set width, height and the grid parameters
        self.setGeometry(900, 700, 5, 6)
        # Call set controls method
        self.set_controls()
        # Connect Backspace button to close our addon.
        self.connect(pyxbmct.ACTION_NAV_BACK, self.close)

    def set_controls(self):
        """Set up UI controls"""
        #title
        username_label = pyxbmct.Label("Kodi Bot Manager", alignment=pyxbmct.ALIGN_CENTER)
        self.placeControl(username_label, 1, 2, -1, 2)
        # Account Settings
        settings_label = pyxbmct.Label('Account Settings:')
        self.placeControl(settings_label, 1, 0, 2, 2)

        # Username
        username_label = pyxbmct.Label(self.get_user_name())
        self.placeControl(username_label, 1, 0, 1, 2)

        # Friends list
        friends_label = pyxbmct.Label('\n\n\n\nFriends list:', alignment=pyxbmct.ALIGN_CENTER)
        self.placeControl(friends_label, 1, 0, 1, 1)
        self.friendlist = pyxbmct.List()
        self.placeControl(self.friendlist, 2, 0, 3, 2)
        # Add items to the list
        self.get_friends_list()

        # Friend Bot List
        bots_label = pyxbmct.Label('\n\n\n\nFriends Bot list:', alignment=pyxbmct.ALIGN_CENTER)
        self.placeControl(bots_label, 1, 3, 1, 1)
        self.botlist = pyxbmct.List()
        self.placeControl(self.botlist, 2, 3, 3, 2)
        self.get_bots_list()
        self.connect(self.botlist, self.check_uncheck)

        # image
        self.image = pyxbmct.Image(os.path.join(_path, 'icon.png'))
        self.placeControl(self.image, 0, 0)
        self.image2 = pyxbmct.Image(os.path.join(_path, 'icon.png'))
        self.placeControl(self.image2, 0, 5)

        #slider
        configuration_label = pyxbmct.Label("\n\nAdditional Configuration:")
        self.placeControl(configuration_label, 3, 0, 3, 2)
        configuration_label = pyxbmct.Label("\n\n\n\nVolume")
        self.placeControl(configuration_label, 3, 0, 3, 2)
        current_volume = float(self.socket_client.get_volume())
        self.slider_value = pyxbmct.Label('\n\n\n\n' + str(current_volume))
        self.placeControl(self.slider_value, 3, 1, 0, 0)
        '''
        comment for future use if needed
        self.slider = pyxbmct.Slider()
        self.slider.setPercent(SLIDER_INIT_VALUE)
        self.placeControl(self.slider, 4, 0, 0, 0, pad_y=1)
        self.connectEventList([pyxbmct.ACTION_MOVE_LEFT,
                               pyxbmct.ACTION_MOVE_RIGHT,
                               pyxbmct.ACTION_MOUSE_DRAG,
                               pyxbmct.ACTION_MOUSE_LEFT_CLICK],
                              self.slider_update)
        '''

    def slider_update(self):
        # Update slider value label when the slider nib moves
        try:
            if self.getFocus() == self.slider:
                self.slider_value.setLabel('\n\n\n\n{:.1F}'.format(self.slider.getPercent()))
        except (RuntimeError, SystemError):
            pass

    def check_uncheck(self):
        list_item = self.botlist.getSelectedItem()
        if list_item.getLabel2() == "checked":
            list_item.setIconImage("")
            list_item.setLabel2("unchecked")
            self.socket_client.deactive_audio_bot(list_item)
        else:
            list_item.setIconImage(_check_icon)
            list_item.setLabel2("checked")
            self.socket_client.active_audio_bot(list_item)

    def radio_update(self):
        # Update radiobutton caption on toggle
        if self.radiobutton.isSelected():
            self.radiobutton.setLabel('On')
        else:
            self.radiobutton.setLabel('Off')

    def get_friends_list(self):
        friends = self.socket_client.get_friends()
        friends_list = self._get_list(friends)
        for friend in friends_list:
            self.friendlist.addItem(xbmcgui.ListItem(friend, iconImage=_person_icon))

    def get_bots_list(self):
        audio_bots = self.socket_client.get_audio_bots()
        audio_bots_lists = self._get_list(audio_bots)
        for bot in audio_bots_lists:
            active_image = None
            bot_details = bot.split(",")
            bot_name = bot_details[0]
            bot_active = bot_details[1]
            if bot_active:
                active_image = _check_icon
            self.botlist.addItem(xbmcgui.ListItem(bot_name, iconImage=active_image))

    def get_user_name(self):
        # socket client
        return "\n\n" + self.socket_client.get_account()

    def _get_list(self, string_to_split):
        list_to_return = string_to_split.split(";")
        del list_to_return[-1]
        return list_to_return


if __name__ == '__main__':
    myaddon = MyAddon('Boty')
    myaddon.doModal()
    del myaddon
