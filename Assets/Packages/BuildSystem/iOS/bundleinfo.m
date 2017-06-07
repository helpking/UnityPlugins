
void GetBundleVersionShortString(char *buffer, int size)
{
	NSString *version = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleShortVersionString"];
	[version getCString:buffer maxLength:size encoding:NSASCIIStringEncoding];
}

void GetBundleVersion(char *buffer, int size)
{
	NSString *version = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleVersion"];
	[version getCString:buffer maxLength:size encoding:NSASCIIStringEncoding];
}

void GetBundleId(char *buffer, int size)
{
	NSString *bid = [[NSBundle mainBundle] bundleIdentifier];
	[bid getCString:buffer maxLength:size encoding:NSASCIIStringEncoding];
}