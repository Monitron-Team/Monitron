FROM fedora:23
RUN dnf install 'dnf-command(config-manager)' -y
RUN rpm --import "http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
RUN dnf config-manager --add-repo http://download.mono-project.com/repo/centos/
RUN dnf install mono-complete -y
ADD Node.tar.gz /opt/
COPY wait_for_config.sh /
CMD /wait_for_config.sh
