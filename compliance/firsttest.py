from netaddr import *
import numpy as np

ip = IPNetwork('192.0.2.0/24')
ip2 = IPNetwork('192.0.2.0/25')
if ip in ip2:
    print(ip.cidr)

set = IPSet(IPNetwork('192.0.2.0/30'))
set.add(IPNetwork('10.0.2.0/30'))

range = IPRange('10.0.2.3', '10.0.2.7')
set.remove(range)
set2 = IPSet(IPNetwork('192.1.2.0/30'))

#print(set & set2)
#print(set.isdisjoint(set2))
#for ip in set:
    #print(ip)

v = np.ones(3)
v[2]=10
M = np.ones((3, 3))
M[1,0]=2
#print(M)
#print(np.matmul(v, M))
#print(np.transpose(v))
#print(v)

set3 = IPSet(IPRange('192.168.0.15', '192.168.99.255'))
set3.add(IPNetwork('192.168.0.0/28'))
print(set3)