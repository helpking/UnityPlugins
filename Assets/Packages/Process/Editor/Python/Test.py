#!/usr/bin/env python
# -*- coding: UTF-8 -*-

import sys
import os
import json

print "TestOK"

def Init() :
	return json.dumps(sys.path)

if __name__ == '__main__':
	result = Init()
	print result