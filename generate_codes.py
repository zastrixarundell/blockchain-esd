from hashlib import sha256
from random import choices
from string import ascii_uppercase, digits

dict = {}

while len(dict) < 10:
	generated = ''.join(choices(ascii_uppercase + digits, k = 20))
	hashed_string = sha256(generated.encode('utf-8')).hexdigest()
	if hashed_string[0:3] == "000":
		dict[generated] = hashed_string
		print("Example data: " + generated)
		print("Example hash: " + hashed_string + "\n")