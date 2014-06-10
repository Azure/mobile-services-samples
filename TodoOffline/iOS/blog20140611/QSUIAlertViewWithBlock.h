//
//  QSUIAlertViewWithBlock.h
//  blog20140611
//
//  Created by Carlos Figueira on 6/9/14.
//  Copyright (c) 2014 MobileServices. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef void (^QSUIAlertViewBlock) (NSInteger index);

@interface QSUIAlertViewWithBlock : NSObject <UIAlertViewDelegate>

- (id) initWithCallback:(QSUIAlertViewBlock)callback;
- (void) showAlertWithTitle:(NSString *)title message:(NSString *)message cancelButtonTitle:(NSString *)cancelButtonTitle otherButtonTitles:(NSArray *)otherButtonTitles;

@end

