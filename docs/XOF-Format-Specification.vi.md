# XOF — Xiangqi Open Format (Định dạng Mở cho Cờ Tướng)

[English](XOF-Format-Specification.md) | [简体中文](XOF-Format-Specification.zh-CN.md) | **Tiếng Việt**

**Đặc tả, phiên bản 1.0 (2026-11-16)**

XOF là định dạng file văn bản thuần dùng để lưu các ván Cờ Tướng. Định dạng này lưu được
nước đi, các biến lồng nhau, lời bình cho từng nước, thời điểm của từng nước và mọi thông tin
ván đấu bạn muốn thêm vào. XOF thay thế bản nháp tiếng Việt năm 2025
(`XOF Format Specifications.pdf`). Chỗ nào hai bản khác nhau thì theo tài liệu này (xem §3.9).

Mục lục

1. [Các định dạng hiện nay có vấn đề gì](#1-các-định-dạng-hiện-nay-có-vấn-đề-gì)
2. [Thế giới cần một định dạng tốt hơn: vì sao chọn XOF](#2-thế-giới-cần-một-định-dạng-tốt-hơn-vì-sao-chọn-xof)
3. [Đặc tả định dạng XOF](#3-đặc-tả-định-dạng-xof)
4. [Lập trình với XOF](#4-lập-trình-với-xof)

---

## 1. Các định dạng hiện nay có vấn đề gì

Một biên bản ván cờ gồm hai tầng: **cách ghi nước đi** (một nước được viết thế nào) và **định
dạng file** (cả ván, cùng thông tin ván, các biến và lời bình, được lưu thế nào). Hiện nay cả
hai tầng đều có vấn đề nghiêm trọng.

### 1.1 Cách ghi nước đi: mơ hồ, không thống nhất và phụ thuộc luật

Cách ghi truyền thống (`C2=5`, `N8+7`, 炮二平五, 马8进7 …) là cách mà người chơi thực sự dùng.
Nó dễ hình dung trên bàn cờ. Nhưng mỗi liên đoàn lại viết một kiểu. Chúng tôi đã đối chiếu các
sách luật đã xuất bản (WXF 2018, AXF 1996, Luật Việt Nam 2004 và 2021, Luật Trung Quốc 2020) và
thấy các vấn đề sau:

1. **Bỏ ký hiệu trước/sau làm nước đi bị mơ hồ.** Một cuốn sách AXF in `C9+1` ở một thế cờ mà
   bên Xanh có *hai* quân Pháo trên lộ 9, cách nhau hai hàng, và *cả hai* đều tiến được một
   hàng. Không ai biết được Pháo nào đã đi; theo cách viết WXF thì phải là `+C+1`. Một nước
   như vậy chỉ giải được nhờ may mắn, khi tình cờ chỉ một trong hai quân có nước đi hợp lệ.
   Vì thế người đọc (người hay chương trình) cần có thế cờ và bộ sinh nước đi hợp lệ thì mới
   hiểu được dòng chữ.
2. **"Trước / giữa / sau" chỉ xử lý được ba quân.** Một lộ có thể có tới năm quân Chốt. Ký
   hiệu truyền thống không gọi tên được Chốt thứ 4 và thứ 5.
3. **Cùng một dòng chữ mang nghĩa khác nhau theo các luật khác nhau.** Ví dụ, `P2-5` trong cách
   ghi của Việt Nam 2004 là nước đi *ngang*, còn trong cách ghi 2021 cùng chuỗi đó lại là nước
   *lùi*. Muốn đọc một ván, bạn phải biết (hoặc đoán) nó được ghi theo sách luật nào, và điều đó
   phải đúng cho cả ván.
4. **Ký hiệu quân hai chữ cái** (ví dụ `Tg` của Việt Nam 2004) làm các nước đi dài ngắn khác
   nhau và phá vỡ cách phân tích đơn giản.
5. **Chính các sách chính thức cũng không nhất quán.** Sách WXF ghi cùng một tình huống (hai Xe
   trên một lộ) khi thì `R4-2`, khi thì `+R-2`. Sĩ và Tượng dùng số lộ thay cho trước/sau. Một
   cuốn sách lúc dùng chữ hoa, lúc dùng chữ thường cho bên Xanh. Không luật nào nói phải làm gì
   khi *hai* lộ đều có Chốt chồng.
6. **Trộn lẫn các hệ chữ số và chữ viết.** Biên bản Trung Quốc ghi lộ của bên Đỏ bằng 一…九 và
   của bên Đen bằng 1–9, đôi khi bằng chữ số toàn khổ １…９. Họ còn trộn chữ Phồn thể và Giản thể
   (車/车, 進/进, 後/后).
7. **Lỗi in ấn không thể phát hiện từ dòng chữ.** Trong một cuốn sách AXF chúng tôi tìm thấy 13
   hình có nước đi sai (sai chữ quân hoặc sai con số). Chúng chỉ lộ ra khi đi lại các nước trên
   bàn cờ.

Các cách ghi theo toạ độ như ICCS (`h2e2`) thì không mơ hồ, nhưng người khó đọc, và hầu như
không người chơi nào ghi chép hay suy nghĩ bằng cách đó.

### 1.2 Định dạng file: đóng, phẳng hoặc định nghĩa lỏng lẻo

- **Định dạng nhị phân đóng.** Phần lớn chương trình Cờ Tướng lưu ván bằng định dạng nhị phân
  riêng (XQF, CCBridge CBL/CBR, …). Một số còn được mã hoá hoặc không có tài liệu. Chỉ chương
  trình đã ghi file mới đọc được nó. Chương trình chết thì kho ván chết theo.
- **PGN cho Cờ Tướng không có biến.** Trên thực tế, các file PGN Cờ Tướng bạn gặp không bao giờ
  chứa nhánh rẽ. Phần phân tích, phần quý giá nhất của một ván có bình luận, bị mất.
- **Phần nước đi trong PGN chưa được chuẩn hoá cho Cờ Tướng.** Mỗi chương trình ghi dùng chữ cái
  quân, ngôn ngữ và cách ghi riêng (xem §1.1). File không cho biết dùng bộ ký hiệu nào, nên người
  đọc phải được báo bộ ký hiệu bằng tay trước khi nhập.
- **PGN khó phân tích cho đúng.** Biến, lời bình, số thứ tự nước và ký hiệu đánh giá đều chung
  một dòng văn bản tự do. Viết một bộ đọc đúng tốn công hơn rất nhiều so với mức cần thiết.
- **Không có chỗ cho thời gian nước đi.** Bàn cờ điện tử và chơi trực tuyến ghi lại *thời điểm*
  mỗi nước được đi, nhưng không có chỗ chuẩn nào để lưu.

---

## 2. Thế giới cần một định dạng tốt hơn: vì sao chọn XOF

Tên **XOF** là viết tắt của **Xiangqi Open Format** (Định dạng Mở cho Cờ Tướng). Mục tiêu: ai
cũng đọc được, ai cũng ghi được, không có gì mơ hồ.

| Vấn đề hiện nay | XOF giải quyết thế nào |
|---|---|
| Định dạng nhị phân đóng | Văn bản UTF-8 thuần. Mở được bằng mọi trình soạn thảo văn bản, đọc được mà không cần phần mềm. |
| Nước đi mơ hồ (§1.1 mục 1, 2, 5) | **Luôn ghi số lộ.** Khi hai quân cùng loại trở lên chung một lộ, **bắt buộc** có tiền tố vị trí `1`…`5`. Cách này xử lý được cả năm quân Chốt, và mọi nước đi đều giải mã được từ thế cờ mà không phải đoán. |
| Cùng dòng chữ, nghĩa khác nhau giữa các sách luật (mục 3) | Mỗi dấu chỉ một nghĩa: `+` tiến, `-` lùi (thoái), `=` đi ngang (bình). Không bao giờ phụ thuộc sách luật. |
| Chữ cái quân khác nhau theo ngôn ngữ (mục 4, 6) | Mỗi ván tự khai báo **Pieces Key** (bộ ký hiệu quân, 7 ký hiệu một ký tự). Có thể dùng tiếng Anh, tiếng Việt, tiếng Trung hay bất kỳ ngôn ngữ nào và đọc lại không mơ hồ. |
| Không có biến (PGN) | Biến sâu bao nhiêu cũng được, một nước có bao nhiêu phương án thay thế cũng được. |
| Khó phân tích | **Một nước = một dòng**, các cột cách nhau bằng tab. Bộ đọc chỉ là một bộ tách dòng cộng một hàm đệ quy nhỏ (§4). |
| Không có thời gian nước đi | Mỗi dòng nước đi có cột thời điểm riêng. |
| Lời bình lẫn vào dòng nước đi | Mỗi dòng nước đi có cột lời bình riêng. |
| Thông tin ván đấu tuỳ tiện | 12 tên header bắt buộc cộng thêm bao nhiêu header riêng của bạn cũng được. |

Dòng văn bản thuần còn mang lại các lợi ích khác:

- **Thân thiện với quản lý phiên bản.** Mỗi dòng một nước nên `git diff` cho kết quả gọn gàng.
  Sửa một nước hoặc một lời bình chỉ hiện thành thay đổi một dòng.
- **Tìm kiếm được bằng công cụ chuẩn.** `grep` hay bất kỳ công cụ tìm văn bản nào cũng tìm được
  một nước đi, một lời bình hay một kỳ thủ trong hàng nghìn file.
- **Thân thiện với cơ sở dữ liệu.** Cả ván là một chuỗi. Lưu vào một cột văn bản và phân tích lại
  bằng chính bộ đọc đó (§3.8).
- **Dễ sửa bằng tay.** Một lỗi in ấn từ sách có thể sửa ngay trong trình soạn thảo văn bản.

---

## 3. Đặc tả định dạng XOF

Các từ khoá **PHẢI** (MUST), **KHÔNG ĐƯỢC** (MUST NOT), **NÊN** (SHOULD), **KHÔNG NÊN**
(SHOULD NOT) và **CÓ THỂ** (MAY) được dùng theo nghĩa của RFC 2119.

### 3.1 Bảng mã và ký tự xuống dòng

- File **PHẢI** được mã hoá bằng **UTF-8**. Chương trình ghi **KHÔNG NÊN** ghi BOM.
- Ký tự xuống dòng **CÓ THỂ** là `\r\n`, `\n` hoặc `\r`. Chương trình đọc **PHẢI** chấp nhận cả ba.
- Chương trình đọc **PHẢI** bỏ qua dòng trống. Chương trình đọc **NÊN** cắt khoảng trắng ở đầu và
  cuối mỗi dòng trước khi xử lý. Đừng cắt các tab *bên trong* dòng nước đi, vì chúng ngăn cách
  các cột.

### 3.2 Cấu trúc file

Một file chứa một hoặc nhiều **ván**. Mỗi ván có một **phần header** và một **phần nước đi**:

```
<phần header của ván 1>
---------                       ← dòng ngăn phần: đúng 9 dấu gạch
<phần nước đi của ván 1>
------------------              ← dòng ngăn ván: đúng 18 dấu gạch
<phần header của ván 2>
---------
<phần nước đi của ván 2>
------------------
```

- **Dòng ngăn phần** là dòng chứa đúng **9** dấu gạch ngang (`---------`).
- **Dòng ngăn ván** là dòng chứa đúng **18** dấu gạch ngang (`------------------`). Chương trình
  ghi **PHẢI** đặt nó **sau** mọi ván, kể cả ván cuối cùng. Nó không bao giờ xuất hiện ở đầu file.
- Chương trình đọc **NÊN** vẫn chấp nhận ván cuối không có dòng ngăn ván theo sau.
- Giá trị header và lời bình **KHÔNG ĐƯỢC** chứa chuỗi từ 9 dấu gạch ngang liên tiếp trở lên.
  Nếu không, chương trình đọc có thể nhầm nó là dòng ngăn.

### 3.3 Phần header

Mỗi dòng một header:

```
<Header Name>: <Header Value>
```

- Tên và giá trị cách nhau bằng **dấu hai chấm** (`:`). Khoảng trắng ở đầu và cuối của cả hai
  được bỏ qua.
- **Tên hoặc giá trị header KHÔNG ĐƯỢC chứa dấu hai chấm.** Chương trình ghi cần lưu giờ trong
  header thì phải thay dấu hai chấm bằng ký tự khác (ví dụ `20?53?00` hoặc `20.53.00`). Thời điểm
  của nước đi không bị ảnh hưởng, vì dòng nước đi dùng tab (§3.4).
- Giá trị **CÓ THỂ** để trống. Nhiều ván trong sách cũ không có ngày, địa điểm hay kỳ thủ.

**Header bắt buộc.** Mỗi ván **PHẢI** có đủ 12 tên header sau, kể cả khi giá trị để trống.
Chương trình ghi **NÊN** ghi theo đúng thứ tự này:

| Tên header | Ý nghĩa |
|---|---|
| `Event` | Tên sự kiện hoặc giải đấu (hoặc tên bài, với thế cờ trong sách) |
| `Address` | Nơi diễn ra ván đấu |
| `Date` | Ngày đấu, định dạng khuyến nghị `YYYY-MM-DD` |
| `Round` | Vòng đấu, với giải đấu theo vòng |
| `Game Number` | Số thứ tự ván trong vòng hoặc trận |
| `Red Player` | Kỳ thủ bên Đỏ: một người, một engine, một robot… |
| `Blue Player` | Kỳ thủ bên Xanh (bên mà nhiều sách gọi là "Đen") |
| `Red Team` | Đội, câu lạc bộ hoặc liên đoàn của kỳ thủ bên Đỏ |
| `Blue Team` | Đội, câu lạc bộ hoặc liên đoàn của kỳ thủ bên Xanh |
| `Starting FEN` | Thế cờ xuất phát ở dạng FEN (xem bên dưới) |
| `Result` | `1-0` Đỏ thắng, `0-1` Xanh thắng, `1/2-1/2` hoà, `*` không rõ hoặc chưa kết thúc |
| `Pieces Key` | Ký hiệu dùng cho các quân trong phần nước đi (§3.5) |

`Starting FEN` của thế cờ khởi đầu bình thường là:

```
rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1
```

Ván chấp quân, tàn cuộc và bài tập dùng thế cờ riêng. Bên đi trong FEN (`w` = Đỏ, `b` = Xanh)
quyết định ai đi nước 1.

**Header riêng.** Sau các header bắt buộc, một ván **CÓ THỂ** có thêm bao nhiêu header cũng được,
theo cùng quy tắc `Name: Value` (ví dụ `ECCO`, `Commentator`, `Time Control`, `Source`). Chương
trình đọc **PHẢI** đọc và giữ lại **tất cả** header, kể cả những header nó không biết.

### 3.4 Phần nước đi

Mỗi dòng ghi **một nước của một bên** (một ply). Dòng nước đi có 6 cột cách nhau bằng ký tự
**tab**:

```
[ML|VA] <TAB> [Number] <TAB> [Move] <TAB> [R|B] <TAB> [Date time] <TAB> [Comment]
```

| Cột | Nội dung |
|---|---|
| 1 | `ML` cho nước thuộc mạch chính, `VA` cho nước nằm trong một biến |
| 2 | Số thứ tự ply, đếm từ 1. Mỗi dòng là một ply, nên Đỏ và Xanh có số riêng (1 Đỏ, 2 Xanh, 3 Đỏ…). |
| 3 | Nước đi, viết như §3.6 bằng Pieces Key của ván |
| 4 | Bên đi nước đó: `R` Đỏ, `B` Xanh |
| 5 | Thời điểm đi nước, **CÓ THỂ** để trống. Định dạng khuyến nghị `YYYY-MM-DD HH:MM:SS` (ở đây được dùng dấu hai chấm). Hữu ích cho ván từ bàn cờ điện tử hoặc chơi trực tuyến, ví dụ để nghiên cứu tốc độ của kỳ thủ. |
| 6 | Lời bình cho nước đi, **CÓ THỂ** để trống. **KHÔNG ĐƯỢC** chứa tab hay ký tự xuống dòng. Chương trình ghi phải thay chúng bằng dấu cách. |

Cột 5 và 6 có thể để trống, nhưng chương trình ghi **NÊN** luôn ghi đủ 6 cột (cột trống chỉ là
các tab liên tiếp). Chương trình đọc **PHẢI** chấp nhận dòng chỉ có 4 cột đầu.

Ví dụ (Pieces Key tiếng Anh):

```
ML	1	C2=5	R	2025-07-08 20:53:00	Central cannon
```

**Kết quả chỉ được lưu trong header `Result`.** Chương trình ghi **KHÔNG ĐƯỢC** thêm dòng kết quả
(`1-0`, `0-1`, …) sau nước cuối cùng. Phần nước đi kết thúc bằng dòng nước đi cuối cùng (hoặc
`VARIANT]`), tiếp theo là dòng ngăn ván.

### 3.5 Pieces Key (bộ ký hiệu quân)

Header `Pieces Key` liệt kê ký hiệu của 7 loại quân dưới dạng các cặp `<Tên quân> = <ký hiệu>`,
cách nhau bằng dấu phẩy, theo thứ tự này:

```
Pieces Key: King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P
```

- Tên quân là các từ tiếng Anh cố định: `King` (Tướng), `Assistant` (Sĩ), `Elephant` (Tượng),
  `Rook` (Xe), `Cannon` (Pháo), `Knight` (Mã), `Pawn` (Chốt).
- Mỗi ký hiệu **PHẢI** là **đúng một ký tự**, và 7 ký hiệu **PHẢI** khác nhau.
- Ký hiệu **KHÔNG ĐƯỢC** là chữ số, `+`, `-`, `=`, tab hay khoảng trắng. Chữ cái Latin **NÊN**
  viết hoa.
- Hai bên dùng cùng một ký hiệu. Cột 4 (`R` / `B`) cho biết bên nào đi.

Các bộ ký hiệu khuyến nghị:

| Ngôn ngữ | Giá trị Pieces Key |
|---|---|
| Tiếng Anh | `King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P` |
| Tiếng Việt | `King = T, Assistant = S, Elephant = V, Rook = X, Cannon = P, Knight = M, Pawn = C` |
| Tiếng Trung (Giản thể, cả hai bên dùng chữ của bên Đỏ) | `King = 帅, Assistant = 仕, Elephant = 相, Rook = 车, Cannon = 炮, Knight = 马, Pawn = 兵` |

Mọi bộ khác đều hợp lệ nếu tuân theo các quy tắc trên. Chương trình đọc không bao giờ được tự giả
định bộ ký hiệu; nó phải luôn dùng bộ ký hiệu mà ván đã khai báo.

### 3.6 Cách viết nước đi

Một nước đi được viết như sau:

```
[position prefix] <piece> <street> <sign> <number>
```

tức là: [tiền tố vị trí] <quân> <lộ> <dấu> <số>

- **Lộ** được đánh số **từ 1 đến 9 tính từ bên tay phải của mỗi người chơi**, theo cách thông
  thường của Cờ Tướng. Đỏ đếm từ bên phải của Đỏ, Xanh đếm từ bên phải của Xanh.
- **Dấu** (cố định trong mọi ngôn ngữ):
  - `+` **tiến**: đi về phía đối phương
  - `-` **lùi** (thoái): đi về phía bên mình
  - `=` **ngang** (bình): đi dọc theo cùng một hàng
- **Con số sau dấu:**
  - **Xe, Pháo, Chốt, Tướng**: sau `+` / `-` là **số hàng đã đi**. Sau `=` là **lộ mà quân đi
    tới**.
  - **Mã, Tượng, Sĩ**: sau `+` / `-` là **lộ mà quân đi tới**. Các quân này không bao giờ đi
    ngang, nên không bao giờ dùng `=` cho chúng.

Ví dụ (bộ ký hiệu tiếng Việt: `P` Pháo, `M` Mã):

| Nước đi | Ý nghĩa |
|---|---|
| `P2=5` | Pháo ở lộ 2 đi ngang sang lộ 5 |
| `M8+7` | Mã ở lộ 8 tiến tới lộ 7 |
| `P8-2` | Pháo ở lộ 8 lùi 2 hàng |

**Nhiều quân cùng loại trên một lộ.** Khi hai quân cùng loại, cùng bên trở lên đứng trên **cùng
một lộ**, nước đi **PHẢI** bắt đầu bằng tiền tố vị trí từ `1` đến `5`:

- `1` là quân **xa hàng cuối của bên mình nhất** (quân tiến xa nhất), rồi đến `2`, `3`, `4`, và
  `5` là quân **gần hàng cuối của bên mình nhất**.
- Quy tắc này áp dụng cho Sĩ, Tượng, Xe, Pháo, Mã và tới năm quân Chốt. Số lộ vẫn được ghi.
- Ví dụ (bộ ký hiệu tiếng Anh): `1C4+2` là Pháo trước ở lộ 4 tiến 2 hàng. `2A4+5` là Sĩ sau ở
  lộ 4 tiến tới lộ 5. `3P5=4` là Chốt thứ ba ở lộ 5 đi ngang sang lộ 4.
- **KHÔNG ĐƯỢC** ghi tiền tố khi quân đứng một mình trên lộ của nó.

Vì lộ luôn được ghi và tiền tố là bắt buộc, **mỗi dòng chữ nước đi ứng với đúng một nước** trên
bàn cờ. Điều này vẫn đúng ngay cả khi hai lộ khác nhau đều có quân chồng.

### 3.7 Biến

Một biến là phương án thay thế cho nước được ghi **ngay trước** nó. Ví dụ dưới đây lấy từ một
file tàn cuộc thật và dùng bộ ký hiệu tiếng Việt (`S` Sĩ, `P` Pháo):

```
ML	7	C7+1	R
ML	8	1S4-5	B			← nước 8 của mạch chính
[VARIANT
VA	8	2S4+5	B			← phương án thay cho nước 8
VA	9	P4=5	R
VARIANT]
ML	9	...
```

- Một biến nằm giữa dòng `[VARIANT` và dòng `VARIANT]`. Nó được ghi **ngay sau** nước mà nó thay
  thế.
- Nước đầu tiên của biến có **cùng số ply và cùng bên** với nước mà nó thay thế. Sau đó việc đánh
  số tiếp tục bên trong biến (9, 10, …).
- Mọi nước bên trong biến đều dùng `VA` ở cột 1, ở mọi độ sâu.
- **Nhiều phương án**: ghi nhiều khối `[VARIANT … VARIANT]` nối tiếp nhau, ngay sau cùng một nước.
  Thứ tự của chúng là thứ tự ưu tiên.
- **Biến lồng nhau**: một khối `[VARIANT … VARIANT]` có thể đi sau bất kỳ dòng `VA` nào. Khi đó
  nó là phương án thay cho nước đó bên trong biến. Không giới hạn độ sâu.
- Chương trình ghi **KHÔNG ĐƯỢC** ghi biến rỗng (khối không có dòng nước đi nào).
- Sau dòng đóng `VARIANT]`, mạch chính (hoặc biến cha) tiếp tục với nước kế tiếp sau nước đã bị
  thay thế.

### 3.8 Lưu XOF trong cơ sở dữ liệu

Cả một ván, từ dòng header đầu tiên đến dòng ngăn ván, có thể lưu trong một trường văn bản duy
nhất cùng với các ký tự xuống dòng. Đọc trường đó ra và tách thành các dòng giống hệt như với file.

### 3.9 Thay đổi so với bản nháp 2025

- Bản nháp kết thúc phần nước đi bằng một dòng kết quả. **Dòng này không còn được ghi nữa.** Kết
  quả chỉ nằm trong header `Result`, và `Result` nay là header bắt buộc.
- Bản nháp dùng khối `[VARIANT … VARIANT]` cùng với mã cấp `Vx`. Nay chỉ dùng `VA`, và việc lồng
  nhau được thể hiện bằng chính các khối.
- Bộ ký hiệu tiếng Trung khuyến nghị nay dùng chữ Giản thể cho cả hai bên (帅仕相车炮马兵).

### 3.10 Ví dụ đầy đủ

Hai ván trong một file (bộ ký hiệu tiếng Anh; `→|` đánh dấu một tab):

```
Event: Example Open
Address: Hanoi
Date: 2026-11-16
Round: 1
Game Number: 1
Red Player: Player A
Blue Player: Player B
Red Team:
Blue Team:
Starting FEN: rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1
Result: *
Pieces Key: King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P
ECCO: B10
---------
ML→|1→|C2=5→|R→|2026-11-16 09:00:05→|Central cannon→|
ML→|2→|N8+7→|B→|2026-11-16 09:00:31→|→|
[VARIANT
VA→|2→|C8=5→|B→|→|Same-direction cannons→|
VA→|3→|N2+3→|R→|→|→|
[VARIANT
VA→|3→|N8+7→|R→|→|→|
VARIANT]
VA→|4→|N8+7→|B→|→|→|
VARIANT]
ML→|3→|N2+3→|R→|2026-11-16 09:01:10→|→|
ML→|4→|R9=8→|B→|2026-11-16 09:01:42→|→|
------------------
Event: Second game
...
---------
...
------------------
```

---

## 4. Lập trình với XOF

Đọc và ghi XOF rất đơn giản. Đó chỉ là phân tích văn bản, không phải viết bố cục nhị phân, cơ
chế thoát ký tự hay bộ tách từ (tokenizer). Bộ đọc chỉ cần:

1. **Tách file thành các ván** tại những dòng bằng đúng dòng ngăn ván 18 dấu gạch.
2. **Tách mỗi ván thành header và nước đi** tại dòng ngăn phần 9 dấu gạch.
3. **Header:** tách mỗi dòng tại dấu hai chấm, rồi cắt khoảng trắng hai phần.
4. **Nước đi:** tách mỗi dòng tại các tab để được 6 cột.
5. **Biến:** một **hàm đệ quy**. Khi dòng sau một nước là `[VARIANT`, gọi hàm đó. Hàm gom các
   dòng `VA` vào một danh sách cho tới khi gặp `VARIANT]`, và tự gọi lại chính nó mỗi khi gặp một
   `[VARIANT` khác. Mỗi danh sách hàm trả về được gắn vào nước ngay trước khối đó.

Bằng mã giả:

```
parseVariant(lines, i, parentMove):
    i = i + 1                                  # bỏ qua "[VARIANT"
    list = []
    while lines[i] != "VARIANT]":
        if lines[i] == "[VARIANT":
            i = parseVariant(lines, i, last(list))   # lồng nhau: thay cho nước cuối
        else:
            list.add(parseMoveLine(lines[i]))
        i = i + 1
    parentMove.variants.add(list)              # một nước có thể có nhiều biến
    return i                                   # chỉ số của "VARIANT]"
```

Như vậy một nước giữ một **danh sách các biến**, và mỗi biến là một danh sách nước đi
(`List<List<Move>>`), các nước này lại có thể có biến của riêng chúng.

Ghi thì làm ngược lại: ghi các header, dòng 9 dấu gạch, rồi từng dòng nước đi. Sau một nước có
biến, ghi mỗi biến dưới dạng `[VARIANT`, các nước của nó (đệ quy) và `VARIANT]`. Kết thúc bằng
dòng 18 dấu gạch.

### 4.1 Bản cài đặt tham chiếu: XOFWorker (C#)

**XOFWorker** là thư viện C# (.NET) mã nguồn mở mà [SuperXiangQi](https://superxiangqi.appsixtyone.com/)
dùng để đọc và ghi XOF:

> **https://github.com/KevinDev166/XOFWorker**

| Lớp | Vai trò |
|---|---|
| `XOFReader` | Đọc file từng ván một và phát một sự kiện cho mỗi ván đã phân tích (và cho mỗi lỗi), nhờ vậy xử lý được file lớn mà không phải nạp hết vào trước |
| `XOFWriter` | Ghi một ván hoặc một danh sách ván ra file |
| `XOFGame` | Một ván: các header, Pieces Key và các nước của mạch chính |
| `XOFMove` | Một dòng nước đi. `Variants` là một `List<List<XOFMove>>`. |
| `XOFHeader` | Một cặp `Name: Value` |
| `XOFPiecesKey` | Pieces Key, có sẵn các bộ tiếng Anh / tiếng Việt / tiếng Trung |
| `XOFKeywords` | Mọi dòng ngăn, từ khoá và tên header bắt buộc gom vào một chỗ |

Bộ phân tích biến đệ quy là `XOFReader.ParseVariant`. Xem mã nguồn của dự án để biết chi tiết.

XOFWorker kiểm tra **cú pháp** của file. Nó không kiểm tra nước đi có **hợp lệ** trên bàn cờ hay
không, vì việc đó cần một bộ sinh nước đi Cờ Tướng. Ứng dụng của bạn nên đi lại các nước từ
`Starting FEN` và tự kiểm tra.

### 4.2 Các ngôn ngữ khác

Hiện XOFWorker chỉ có bản C#. Viết bộ đọc bằng ngôn ngữ khác chỉ tốn chút công nếu bạn làm theo
§4 và mã tham chiếu. Nếu nhiều lập trình viên cần, các bản chuyển sang ngôn ngữ khác có thể được
phát hành sau.
