-- db/schema.sql  (Supabase > SQL Editor'de calistir)
create table olay (
  id         bigint generated always as identity primary key,
  oturum_id  uuid        not null,
  oyuncu_id  uuid        not null,   -- tarayicida uretilen rastgele kimlik
  surum      text        not null check (surum in ('v1', 'v2')),
  sira       int         not null,   -- oturum ici olay sirasi
  t_ms       int         not null,   -- oyun acildiktan beri gecen sure
  tur        text        not null,   -- olay sozlugundeki 8 turden biri
  bolum      smallint    not null,
  x          real,
  y          real,
  neden      text,                   -- olumde: cukur | diken | dusman
  alindi     timestamptz not null default now()
);

alter table olay enable row level security;

-- Tarayicidaki anahtar herkese acik: oyun yalnizca EKLEYEBILIR, okuyamaz.
create policy yalniz_ekleme on olay for insert to anon with check (true);

-- Oturum basina hizli ozet (kontrol icin; analiz pandas'ta yapilir).
-- security_invoker olmazsa gorunum RLS'yi atlar ve anahtarla okunabilir.
create view oturum_ozet with (security_invoker = true) as
select oturum_id, oyuncu_id, surum,
       count(*) filter (where tur = 'olum')                      as olum,
       count(*) filter (where tur = 'jeton')                     as jeton,
       coalesce(max(bolum) filter (where tur = 'bolum_bitir'), 0) as son_biten_bolum,
       max(t_ms) / 60000.0                                        as sure_dk,
       bool_or(tur = 'oyun_bitir')                                as bitirdi
from olay
group by oturum_id, oyuncu_id, surum;
