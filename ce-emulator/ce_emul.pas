program ceemul;

{$mode objfpc}{$H+}

uses
  {$IFDEF UNIX}{$IFDEF UseCThreads}
  cthreads,
  {$ENDIF}{$ENDIF}
  Classes, SysUtils, CustApp, zlib, zstream
  { you can add units after this };

type

  { ce_emul }

  ce_emul = class(TCustomApplication)
  protected
    procedure DoRun; override;
  public
    constructor Create(TheOwner: TComponent); override;
    destructor Destroy; override;

  end;

{ ce_emul }

const
  customBase85='0123456789'+
                'ABCDEFGHIJKLMNOPQRSTUVWXYZ'+
                'abcdefghijklmnopqrstuvwxyz'+
                '!#$%()*+,-./:;=?@[]^_{}';

function Base85ToBin(inputStringBase85, BinValue: PChar): integer;
var i,j: integer;
    size : integer;
    a : dword;
begin

  size:=length(inputStringBase85);

  i:=0;
  j:=0;
  while i<size do
  begin
    a:=( pos((inputStringBase85+i)^, customBase85) - 1 )*85*85*85*85;
    inc(i);

    if i<size then
    begin
      a:= a + ( pos((inputStringBase85+i)^, customBase85) - 1 )*85*85*85;
      inc(i);
    end;

    if i<size then
    begin
      a:= a + ( pos((inputStringBase85+i)^, customBase85) - 1 )*85*85;
      inc(i);
    end;

    if i<size then
    begin
      a:= a + ( pos((inputStringBase85+i)^, customBase85) - 1 )*85;
      inc(i);
    end;

    if i<size then
    begin
      a:= a + ( pos((inputStringBase85+i)^, customBase85) - 1 );
      inc(i);

      // 5-tuple
      binvalue[j+0]:= char(  (a shr 24) and $ff  );
      binvalue[j+1]:= char(  (a shr 16) and $ff  );
      binvalue[j+2]:= char(  (a shr  8) and $ff  );
      binvalue[j+3]:= char(  a and $ff           );
      inc(j,4);
    end;
  end;


  case (size mod 5) of
    2: begin // must be padded with three digits (last radix85 digit used)
         a:= a + 84*85*85 + 84*85 + 84;
         binvalue[j+0]:= char(  (a shr 24) and $ff  ); // last three bytes of the output are ignored
         inc(j);
       end;

    3: begin // must be padded with two digits (last radix85 digit used)
         a:= a            + 84*85 + 84;
         binvalue[j+0]:= char(  (a shr 24) and $ff  );
         binvalue[j+1]:= char(  (a shr 16) and $ff  ); // last two bytes of the output are ignored
         inc(j,2);
       end;

    4: begin // must be padded with one digit (last radix85 digit used)
         a:= a                    + 84;
         binvalue[j+0]:= char(  (a shr 24) and $ff  );
         binvalue[j+1]:= char(  (a shr 16) and $ff  );
         binvalue[j+2]:= char(  (a shr  8) and $ff  ); // last byte of the output is ignored
         inc(j,3);
       end;
  end;

  result:=j;

end;


procedure ce_emul.DoRun;
var
  ErrorMsg: String;
  var s: string;
      tfIn: TextFile;
      inputFile:string;
  b: pchar;
  m: TMemorystream;
  dc: Tdecompressionstream;
  size: integer;
  read: integer;

  realsize: dword;
  wasActive: boolean;

  useascii85: boolean;
  fs : TFileStream;
begin
  // quick check parameters

  writeln('ce emulator by vollragm made in pascal >:(');
  inputFile := paramStr(1);
  AssignFile(tfIn, inputFile);
  reset(tfIn);
  while not eof(tfIn) do
    begin
      readln(tfIn, s);

    end;

  writeln('read textfile');

  useascii85:=true;

  size:=(length(s) div 5)*4+(length(s) mod 5);
  getmem(b, size);
  size:=Base85ToBin(pchar(s), b);


  m:=tmemorystream.create;
  m.WriteBuffer(b^, size);
  m.position:=0;
  dc:=Tdecompressionstream.create(m, true);

  dc.read(realsize,sizeof(realsize));

  writeln('realsize:');
  writeln(realsize);

  FreeMemAndNil(b);
  getmem(b, realsize);

  read:=dc.read(b^, realsize);

  fs:=TFileStream.Create('trainerform.dfm', fmCreate);
  fs.Write(b^, read);
  fs.Free;
  writeln('press any key to exit...');
  readln();
  { add your program here }

  // stop program loop
  Terminate;
end;

constructor ce_emul.Create(TheOwner: TComponent);
begin
  inherited Create(TheOwner);
  StopOnException:=True;
end;

destructor ce_emul.Destroy;
begin
  inherited Destroy;
end;



var
  Application: ce_emul;
begin
  Application:=ce_emul.Create(nil);
  Application.Title:='ce-emul';
  Application.Run;
  Application.Free;
end.

