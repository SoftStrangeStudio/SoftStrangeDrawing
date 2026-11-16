#import <Cocoa/Cocoa.h>
#include <stdlib.h>
#include <string.h>
extern "C" const char* SoftStrangePickImage(){
 __block char* result=nullptr;
 void (^pick)(void)=^{NSOpenPanel* panel=[NSOpenPanel openPanel];panel.canChooseDirectories=NO;panel.canChooseFiles=YES;panel.allowsMultipleSelection=NO;panel.allowedFileTypes=@[@"png",@"jpg",@"jpeg"];if([panel runModal]==NSModalResponseOK)result=strdup(panel.URL.path.UTF8String);};
 if([NSThread isMainThread])pick();else dispatch_sync(dispatch_get_main_queue(),pick);
 return result;
}
extern "C" void SoftStrangeFreeString(const char* pointer){free((void*)pointer);}
