

select * from Customer cs where cs.type != 12  and (cs.NgayNo >= 3 and cs.Description is null);

select * from Customer cs 
where cs.type != 12 and( cs.Description like 'End' or cs.DayPaids = (Loan/Price) or cs.NgayNo < 3);

select * from Customer where Code not in(select cs.Code from Customer cs where cs.type = 0  and (cs.NgayNo >= 3 and cs.Description is null)
) and Code not in (select cs.Code from Customer cs where cs.type = 0 and( cs.Description like 'End' or cs.DayPaids = (Loan/Price) or cs.NgayNo < 3))


select type from Customer where Price is null ;

select type from Customer where Price is not null ;

select * from Customer where LEFT(Code, 1) = 'B' ;

select * from camcdb3d_camdothanhluan.Message;

select * from camcdb3d_camdothanhluan.history hs  where hs.CustomerId = 'B001';

select * from Loan where IDCus ='B001';
select * from camcdb3d_camdothanhluan.history hs where hs.CustomerId = 'B001' ;

select * from camcdb3d_camdothanhluan.history hs , Customer cs 
where cs.type = 6
and hs.CustomerId=cs.Code
and convert(date,hs.Ngaydongtien) >= '2018-09-08' and cs.code = 'T1852';

select * from camcdb3d_camdothanhluan.history hs
where hs.CustomerId = 'T1852';

update Customer
set Description = NULL
where Code ='B961';

select * from Customer cs where type = 5;
select * from Users;

select * from Customer where code = 'T1852';
select * from Loan where IDCus = 'B1057';

select * from Message;
select * from Loan;
select * from Users;

update Customer
set type = 5
where Code = 'Y000';
delete Customer where Code = 'Y000'; 

select * from Loan where IDCus = 'T1861';
select * from Customer where Code = 'b1191';
select * from Loan where IDCus = 'b1191';
select * from Customer where Code = 'Y000';


ALTER TABLE Customer
ADD IsDeleted bit NOT NULL DEFAULT 'False'
GO

update Message
set type = 1;

select * from Customer where Price = 0 ;

select * from Customer where type = 1;

delete Customer where IsDeleted = 1;

select * from Customer where IsDeleted = 1;

select * from Loan where IdCus = 'Y000';

delete from Customer where Code = 'Y0000';
select * from Customer where Name like N'Nguyễn Ngọc Duy';
delete from Customer where Name like N'Nguyễn Ngọc Duy';
delete from history;
select * from Customer where Code = 'B1340';

delete from Message where type = 2;
delete from Loan;

use camcdb3d_camdothanhluan;
use camdoanhtu;
select * from Users;

alter table Customer
add Code nvarchar(MAX)

ALTER TABLE Customer ALTER COLUMN Code DROP PERSISTED;

DBCC CHECKIDENT ('Customer', RESEED, 0);

select Sum(cs.Loan)
from customer cs
where cs.StartDate > '2018-06-20' and cs.IsDeleted = 0 and cs.Description is null;

select *
from customer cs, Loan l
where cs.ID = l.IDcus and l.Status = 1;
select * from Loan;
