package binaryChunk

//很多二进制格式都会以固定的魔数（Magic Number）开始，比如Java的class文件，魔数是四字节OxCAFEBABE。Lua二进制chunk的魔数（又叫作签名，Signature）也是4个字节，分别是ESC、L、u、a的ASCII码
type header struct {
	signature       [4]byte //用十六进制表示是OxlB4C7561，写成Go语言字符串字面量是”＼xlbLua” 。
	version         byte
	format          byte
	luacData        [6]byte
	cintSize        byte
	sizetSize       byte
	instructionSize byte
	luaintegerSize  byte
	luaNumberSize   byte
	luacint         int64
	luacNum         float64
}

type binaryChunk struct {
	header                  //头部
	sizeUpValues byte       //主函数 upvalue 数量
	mainFunc     *Prototype //主函数原型
}
