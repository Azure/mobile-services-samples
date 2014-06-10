// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import "QSTodoService.h"
#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import "QSAppDelegate.h"

#pragma mark * Private interace


@interface QSTodoService() <MSFilter>

@property (nonatomic, strong)   MSSyncTable *syncTable;
@property (nonatomic)           NSInteger busyCount;

@end


#pragma mark * Implementation


@implementation QSTodoService

@synthesize items;

+ (QSTodoService *)defaultServiceWithDelegate:(id<MSSyncContextDelegate>)delegate
{
    // Create a singleton instance of QSTodoService
    static QSTodoService* service;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        service = [[QSTodoService alloc] initWithDelegate:delegate];
    });
    
    return service;
}

-(QSTodoService *)initWithDelegate:(id<MSSyncContextDelegate>)syncDelegate
{
    self = [super init];
    
    if (self)
    {
        // Initialize the Mobile Service client with your URL and key
        MSClient *client = [MSClient clientWithApplicationURLString:@"https://blog20140611.azure-mobile.net/"
                                                     applicationKey:@"UiMJHSlktoYGRxGtgkLfgGoZFjqcGS10"];

        // Add a Mobile Service filter to enable the busy indicator
        self.client = [client clientWithFilter:self];

        QSAppDelegate *delegate = (QSAppDelegate *)[[UIApplication sharedApplication] delegate];
        NSManagedObjectContext *context = delegate.managedObjectContext;
        MSCoreDataStore *store = [[MSCoreDataStore alloc] initWithManagedObjectContext:context];

        self.client.syncContext = [[MSSyncContext alloc] initWithDelegate:syncDelegate dataSource:store callback:nil];

        // Create an MSSyncTable instance to allow us to work with the TodoItem table
        self.syncTable = [self.client syncTableWithName:@"TodoItem"];

        self.items = [[NSMutableArray alloc] init];
        self.busyCount = 0;
    }
    
    return self;
}

- (void)refreshDataOnSuccess:(QSCompletionBlock)completion
{
    // Create a predicate that finds items where complete is false
    NSPredicate * predicate = [NSPredicate predicateWithFormat:@"complete == NO"];
    
    MSQuery *query = [self.syncTable queryWithPredicate:predicate];

    // Pulls data from the remote server into the local table. We're only
    // pulling the items which we want to display (complete == NO).
    [self.syncTable pullWithQuery:query completion:^(NSError *error) {
        [self logErrorIfNotNil:error];
        
        [self loadLocalDataWithCompletion:completion];
    }];
}

- (void) loadLocalDataWithCompletion:(QSCompletionBlock)completion
{
    NSPredicate * predicate = [NSPredicate predicateWithFormat:@"complete == NO"];
    MSQuery *query = [self.syncTable queryWithPredicate:predicate];

    [query orderByAscending:@"text"];
    [query readWithCompletion:^(NSArray *results, NSInteger totalCount, NSError *error) {
        [self logErrorIfNotNil:error];

        items = [results mutableCopy];

        // Let the caller know that we finished
        dispatch_async(dispatch_get_main_queue(), ^{
            completion();
        });
    }];
}

-(void)addItem:(NSDictionary *)item completion:(QSCompletionWithIndexBlock)completion
{
    // Insert the item into the TodoItem table and add to the items array on completion
    [self.syncTable insert:item completion:^(NSDictionary *result, NSError *error)
    {
        [self logErrorIfNotNil:error];

        NSUInteger index = [items count];
        [(NSMutableArray *)items insertObject:result atIndex:index];

        // Let the caller know that we finished
        dispatch_async(dispatch_get_main_queue(), ^{
            completion(index);
        });
    }];
}

- (void)updateItem:(NSDictionary *)item atIndex:(NSInteger)index completion:(QSCompletionWithIndexBlock)completion {
    // Cast the public items property to the mutable type (it was created as mutable)
    NSMutableArray *mutableItems = (NSMutableArray *) items;

    // Replace the original in the items array
    [mutableItems replaceObjectAtIndex:index withObject:item];

    // Update the item in the TodoItem table and remove from the items array on completion
    [self.syncTable update:item completion:^(NSError *error) {
        [self logErrorIfNotNil:error];

        NSInteger index = -1;
        if (!error) {
            BOOL isComplete = [[item objectForKey:@"complete"] boolValue];
            NSString *remoteId = [item objectForKey:@"id"];
            index = [items indexOfObjectPassingTest:^BOOL(id obj, NSUInteger idx, BOOL *stop) {
                return [remoteId isEqualToString:[obj objectForKey:@"id"]];
            }];

            if (index != NSNotFound && isComplete)
            {
                [mutableItems removeObjectAtIndex:index];
            }
        }

        // Let the caller know that we have finished
        dispatch_async(dispatch_get_main_queue(), ^{
            completion(index);
        });
    }];
}

- (void)busy:(BOOL)busy
{
    // assumes always executes on UI thread
    if (busy)
    {
        if (self.busyCount == 0 && self.busyUpdate != nil)
        {
            self.busyUpdate(YES);
        }
        self.busyCount ++;
    }
    else
    {
        if (self.busyCount == 1 && self.busyUpdate != nil)
        {
            self.busyUpdate(FALSE);
        }
        self.busyCount--;
    }
}

- (void)logErrorIfNotNil:(NSError *) error
{
    if (error)
    {
        NSLog(@"ERROR %@", error);
    }
}


#pragma mark * MSFilter methods


- (void)handleRequest:(NSURLRequest *)request
                 next:(MSFilterNextBlock)next
             response:(MSFilterResponseBlock)response
{
    // A wrapped response block that decrements the busy counter
    MSFilterResponseBlock wrappedResponse = ^(NSHTTPURLResponse *innerResponse, NSData *data, NSError *error)
    {
        [self busy:NO];
        response(innerResponse, data, error);
    };
    
    // Increment the busy counter before sending the request
    [self busy:YES];
    next(request, wrappedResponse);
}

@end
