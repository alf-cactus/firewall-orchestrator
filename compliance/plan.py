# lets assume we have stored all Zones as IPSet in a vector (ordered) zones
# For this function we get a Source or Destination IPSet
# Returns a vector of 0s and 1s (for each Zone: is IP from IPSet in Zone?)

from netaddr import *
import numpy as np

def ConstructZoneVectorFromIPSet(trafficIPSet, zoneIPSetVector):
    outVector = np.ones(len(zoneIPSetVector))
    for element in range(len(zoneIPSetVector)):
        if trafficIPSet.isdisjoint(zoneIPSetVector[element]):
            outVector[element] = 0
    return outVector

# This function calculates the blocked traffic

def CalculateBlockedTraffic(complianceMatrix, sourceVector, destinationVector):
    return np.multiply(np.matmul(sourceVector, complianceMatrix), destinationVector)

# Testing

complianceMatrix = np.zeros((3,3))
complianceMatrix[1,0] = 1
complianceMatrix[2,0] = 1
complianceMatrix[2,1] = 1
complianceMatrix[2,2] = 1

zoneA = IPSet(IPNetwork('10.0.0.0/24'))
zoneB = IPSet(IPRange('192.168.0.0', '192.168.99.255'))
zoneC = IPSet(IPNetwork('18.1.2.0/30'))
zoneC.add(IPNetwork('18.1.2.9/32'))

zoneIPSetVector = [zoneA]
zoneIPSetVector.append(zoneB)
zoneIPSetVector.append(zoneC)

#print(zoneIPSetVector[1])

sourceIPs1 = IPSet(IPNetwork('192.168.0.1/32'))
destinationIPs1 = IPSet(IPNetwork('10.0.0.0/26'))
destinationIPs1.add(IPNetwork('18.1.2.9/32'))

sourceVector1 = ConstructZoneVectorFromIPSet(sourceIPs1, zoneIPSetVector)
destinationVector1 = ConstructZoneVectorFromIPSet(destinationIPs1, zoneIPSetVector)

print(CalculateBlockedTraffic(complianceMatrix, sourceVector1, destinationVector1))

sourceIPs2 = IPSet(IPRange('18.0.0.0', '255.255.255.255'))
destinationIPs2 = IPSet(IPNetwork('10.0.0.1/32'))
destinationIPs2.add(IPNetwork('192.168.0.1/32'))

sourceVector2 = ConstructZoneVectorFromIPSet(sourceIPs2, zoneIPSetVector)
destinationVector2 = ConstructZoneVectorFromIPSet(destinationIPs2, zoneIPSetVector)

print(CalculateBlockedTraffic(complianceMatrix, sourceVector2, destinationVector2))

destinationIPs3 = IPSet(IPRange('18.0.0.0', '255.255.255.255'))
sourceIPs3 = IPSet(IPNetwork('10.0.0.1/32'))
sourceIPs3.add(IPNetwork('192.168.0.1/32'))

sourceVector3 = ConstructZoneVectorFromIPSet(sourceIPs3, zoneIPSetVector)
destinationVector3 = ConstructZoneVectorFromIPSet(destinationIPs3, zoneIPSetVector)

print(CalculateBlockedTraffic(complianceMatrix, sourceVector3, destinationVector3))
