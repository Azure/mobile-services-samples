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

#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import <WindowsAzureMobileServices/MSCoreDataStore.h>

#import "QSTodoListViewController.h"
#import "QSTodoService.h"
#import "QSAppDelegate.h"
#import "QSUIAlertViewWithBlock.h"


#pragma mark * Private Interface


@interface QSTodoListViewController ()

// Private properties
@property (strong, nonatomic) QSTodoService *todoService;
@property (nonatomic, retain) NSFetchedResultsController *fetchedResultsController;

@property (strong, nonatomic)   NSDictionary *editingItem;

@end


#pragma mark * Implementation


@implementation QSTodoListViewController

#pragma mark * UIView methods


- (void)viewDidLoad
{
    [super viewDidLoad];
    
    // Create the todoService - this creates the Mobile Service client inside the wrapped service
    self.todoService = [QSTodoService defaultServiceWithDelegate:self];
    
    // have refresh control reload all data from server
    [self.refreshControl addTarget:self
                            action:@selector(onRefresh:)
                  forControlEvents:UIControlEventValueChanged];

    NSError *error;
    if (![[self fetchedResultsController] performFetch:&error]) {
        NSLog(@"Unresolved error %@, %@", error, [error userInfo]);
        exit(-1);  // Fail
    }
}

- (NSFetchedResultsController *)fetchedResultsController {
    
    if (_fetchedResultsController != nil) {
        return _fetchedResultsController;
    }
    
    NSFetchRequest *fetchRequest = [[NSFetchRequest alloc] init];
    QSAppDelegate *delegate = (QSAppDelegate *)[[UIApplication sharedApplication] delegate];
    NSManagedObjectContext *context = delegate.managedObjectContext;

    fetchRequest.entity = [NSEntityDescription entityForName:@"TodoItem" inManagedObjectContext:context];
    
    // show only non-completed items
    fetchRequest.predicate = [NSPredicate predicateWithFormat:@"complete == NO"];
    
    // sort by item text
    fetchRequest.sortDescriptors = @[[NSSortDescriptor sortDescriptorWithKey:@"ms_createdAt" ascending:YES]];

    // Note: if storing a lot of data, you should specify a cache for the last parameter
    // for more information, see Apple's documentation: http://go.microsoft.com/fwlink/?LinkId=524591&clcid=0x409
    NSFetchedResultsController *theFetchedResultsController =
        [[NSFetchedResultsController alloc] initWithFetchRequest:fetchRequest
                                            managedObjectContext:context sectionNameKeyPath:nil
                                                       cacheName:nil];
    self.fetchedResultsController = theFetchedResultsController;
    
    _fetchedResultsController.delegate = self;
    
    return _fetchedResultsController;
    
}

- (void)viewDidUnload {
    self.fetchedResultsController = nil;
}

- (void) refresh
{
    [self.refreshControl beginRefreshing];
    
    [self.todoService syncData:^
    {
         [self.refreshControl endRefreshing];
    }];
}


#pragma mark * UITableView methods

- (void)configureCell:(UITableViewCell *)cell atIndexPath:(NSIndexPath *)indexPath
{
    NSManagedObject *item = [self.fetchedResultsController objectAtIndexPath:indexPath];

    // Set the label on the cell and make sure the label color is black (in case this cell
    // has been reused and was previously greyed out
    cell.textLabel.textColor = [UIColor blackColor];
    cell.textLabel.text = [item valueForKey:@"text"];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"Cell";
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    
    [self configureCell:cell atIndexPath:indexPath];
    
    return cell;
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;  // Always a single section
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    id  sectionInfo = [self.fetchedResultsController.sections objectAtIndex:section];
    return [sectionInfo numberOfObjects];
}

#pragma mark * UITextFieldDelegate methods


-(BOOL)textFieldShouldReturn:(UITextField *)textField
{
    [textField resignFirstResponder];
    return YES;
}


#pragma mark * UI Actions


- (IBAction)onAdd:(id)sender
{
    if (_itemText.text.length  == 0)
    {
        return;
    }
    
    NSDictionary *item = @{ @"text" : _itemText.text, @"complete" : @NO };
    [self.todoService addItem:item completion:nil];
    _itemText.text = @"";
}


- (void)onRefresh:(id) sender
{
    [self refresh];
}

#pragma mark * NSFetchedResultsController methods


- (void)controllerWillChangeContent:(NSFetchedResultsController *)controller {
    // The fetch controller is about to start sending change notifications, so prepare the table view for updates.
    dispatch_async(dispatch_get_main_queue(), ^{
        [self.tableView beginUpdates];
    });
}


- (void)controller:(NSFetchedResultsController *)controller didChangeObject:(id)anObject atIndexPath:(NSIndexPath *)indexPath forChangeType:(NSFetchedResultsChangeType)type newIndexPath:(NSIndexPath *)newIndexPath
{
    dispatch_async(dispatch_get_main_queue(), ^{
        UITableView *tableView = self.tableView;
        
        switch(type) {
                
            case NSFetchedResultsChangeInsert:
                [tableView insertRowsAtIndexPaths:[NSArray arrayWithObject:newIndexPath] withRowAnimation:UITableViewRowAnimationFade];
                break;
                
            case NSFetchedResultsChangeDelete:
                [tableView deleteRowsAtIndexPaths:[NSArray arrayWithObject:indexPath] withRowAnimation:UITableViewRowAnimationFade];
                break;
                
            case NSFetchedResultsChangeUpdate:
                [tableView reloadRowsAtIndexPaths:@[indexPath]
                                 withRowAnimation:UITableViewRowAnimationAutomatic];
                
                // note: Apple samples show a call to configureCell here; this is incorrect--it can result in retrieving the
                // wrong index when rows are reordered. For more information, see http://go.microsoft.com/fwlink/?LinkID=524590&clcid=0x409
                // [self configureCell:[tableView cellForRowAtIndexPath:indexPath] atIndexPath:indexPath]; // wrong! call reloadRows instead
                break;
                
            case NSFetchedResultsChangeMove:
                [tableView deleteRowsAtIndexPaths:[NSArray
                                                   arrayWithObject:indexPath] withRowAnimation:UITableViewRowAnimationFade];
                [tableView insertRowsAtIndexPaths:[NSArray
                                                   arrayWithObject:newIndexPath] withRowAnimation:UITableViewRowAnimationFade];
                break;
        }
    });
}


- (void)controller:(NSFetchedResultsController *)controller
    didChangeSection:(id)sectionInfo
    atIndex:(NSUInteger)sectionIndex forChangeType:(NSFetchedResultsChangeType)type
{
    dispatch_async(dispatch_get_main_queue(), ^{
        switch(type) {
                
            case NSFetchedResultsChangeInsert:
                [self.tableView insertSections:[NSIndexSet indexSetWithIndex:sectionIndex] withRowAnimation:UITableViewRowAnimationFade];
                break;
                
            case NSFetchedResultsChangeDelete:
                [self.tableView deleteSections:[NSIndexSet indexSetWithIndex:sectionIndex] withRowAnimation:UITableViewRowAnimationFade];
                break;
                
            case NSFetchedResultsChangeUpdate:
                [self.tableView reloadSections:[NSIndexSet indexSetWithIndex:sectionIndex] withRowAnimation:UITableViewRowAnimationFade];
                break;
                
            default:
                break;
        }
    });
}


- (void)controllerDidChangeContent:(NSFetchedResultsController *)controller {
    // The fetch controller has sent all current change notifications, so tell the table view to process all updates.
    dispatch_async(dispatch_get_main_queue(), ^{
        [self.tableView endUpdates];
    });
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    NSManagedObject *item = [self.fetchedResultsController objectAtIndexPath:indexPath];
    self.editingItem = [MSCoreDataStore tableItemFromManagedObject:item]; // map from managed object to dictionary
    
    [self performSegueWithIdentifier:@"detailSegue" sender:self];
}

- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    if ([[segue identifier] isEqualToString:@"detailSegue"]) {
        QSItemViewController *ivc = (QSItemViewController *) [segue destinationViewController];
        ivc.item = [self.editingItem mutableCopy];
        
        ivc.editCompleteBlock = ^(NSDictionary *editedValue) {
            [self.todoService updateItem:editedValue completion:^(NSUInteger index) {
                self.editingItem = nil;
            }];
        };
    }
}

# pragma mark Conflict handling

- (void)tableOperation:(MSTableOperation *)operation onComplete:(MSSyncItemBlock)completion
{
    [self doOperation:operation complete:completion];
}

-(void)doOperation:(MSTableOperation *)operation complete:(MSSyncItemBlock)completion
{
    [operation executeWithCompletion:^(NSDictionary *item, NSError *error) {
        
        NSDictionary *serverItem = [error.userInfo objectForKey:MSErrorServerItemKey];
        
        if (error.code == MSErrorPreconditionFailed) {
            QSUIAlertViewWithBlock *alert = [[QSUIAlertViewWithBlock alloc] initWithCallback:^(NSInteger buttonIndex) {
                if (buttonIndex == 1) { // Client
                    NSMutableDictionary *adjustedItem = [operation.item mutableCopy];
                    
                    [adjustedItem setValue:[serverItem objectForKey:MSSystemColumnVersion] forKey:MSSystemColumnVersion];
                    operation.item = adjustedItem;
                    
                    [self doOperation:operation complete:completion];
                    return;
                    
                } else if (buttonIndex == 2) { // Server
                    NSDictionary *serverItem = [error.userInfo objectForKey:MSErrorServerItemKey];
                    completion(serverItem, nil);
                } else { // Cancel
                    [operation cancelPush];
                    completion(nil, error);
                }
            }];
            
            NSString *message = [NSString stringWithFormat:@"Client value: %@\nServer value: %@", operation.item[@"text"], serverItem[@"text"]];
            
            [alert showAlertWithTitle:@"Server Conflict"
                              message:message
                    cancelButtonTitle:@"Cancel"
                    otherButtonTitles:[NSArray arrayWithObjects:@"Use Client", @"Use Server", nil]];
        } else {
            completion(item, error);
        }
    }];
}

@end
