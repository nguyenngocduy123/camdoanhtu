use camcdb3d_camdothanhluan

select * from Customer cs
where cs.DayPaids != (
select count(*) from Loan l,Customer c 
where l.Status = 1 and l.Type = 0 and c.Code = l.IDCus and c.Code = cs.Code) and cs.type !=12 ;

select * from Customer cs
where cs.AmountPaid != (
select (count(*)*c.Price) as amountpaid from Loan l,Customer c 
where l.Status = 1 and l.Type = 0 and c.Code = l.IDCus and c.Code = cs.Code  and cs.type !=12 
group by c.Price)


select * from Message;
