echo "Waiting for config file"
while [ ! -f /opt/Node/node.conf ]
do
	sleep 2
done
echo "starting node"
cd /opt/Node
while [ 1 == 1 ]
do
	mono ./Monitron.Node.exe --conf node.conf
done
