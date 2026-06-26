# Guia de Teste — Laser Print & Cut no CorelDRAW

## Pré-requisitos

- CorelDRAW X7 ou superior (x64)
- .NET Framework 4.8 Runtime
- Visual Studio 2022 ou `dotnet` CLI (SDK 4.8)
- Permissão de Administrador (para registrar a DLL)

---

## Passo 1: Compilar o projeto

Abra o **Developer Command Prompt for VS 2022** como Administrador:

```cmd
cd "F:\Macros\Print&Cut v2\LaserPrintCutAddin\src\LaserPrintCut"
dotnet restore
dotnet build -c Release
```

A DLL gerada estará em:
`bin\Release\net48\LaserPrintCut.dll`

---

## Passo 2: Registrar a DLL

Ainda como **Administrador**, execute:

```cmd
cd "F:\Macros\Print&Cut v2\LaserPrintCutAddin"
register.bat
```

O script:
1. Executa `regasm LaserPrintCut.dll /codebase` (registro COM)
2. Adiciona chaves no Registry para cada versão do CorelDRAW encontrada
3. Confirma no final quantas versões foram registradas

---

## Passo 3: Criar uma macro VBA no CorelDRAW para chamar o Add-in

1. Abra o **CorelDRAW**
2. Pressione **Alt + F11** para abrir o Editor VBA
3. No menu **Insert → Module**
4. Cole o código abaixo:

```vba
Option Explicit

' Macro única: chama o Add-in Laser Print & Cut
Public Sub LaserPrintCut()
    On Error GoTo errHandler
    
    Dim addin As Object
    Set addin = CreateObject("LaserPrintCut.Addin")
    
    ' Passa a referência do CorelDRAW para o add-in
    addin.SetCorelApp Application
    
    addin.Startup
    addin.ShowMainWindow
    addin.Shutdown
    Set addin = Nothing
    
    Exit Sub
    
errHandler:
    MsgBox "Erro ao iniciar Laser Print & Cut:" & vbCrLf & _
           Err.Description, vbCritical, "Laser Print & Cut"
End Sub
```

5. Pressione **Ctrl + S** para salvar (dê um nome como `LaserPrintCut`)

---

## Passo 4: Executar a macro

1. No CorelDRAW, vá em **Tools → Macros → Play** (ou **Ferramentas → Macros → Executar**)
2. Selecione `LaserPrintCut.LaserPrintCut` e clique em **Run**
3. A interface WPF do add-in deve aparecer

---

## Passo 5 (Opcional): Adicionar um botão na barra de ferramentas

1. No CorelDRAW: **Tools → Options → Customization → Command Bars**
2. Aba **Commands**
3. Categoria: **Macros**
4. Arraste `LaserPrintCut.LaserPrintCut` para a barra de ferramentas desejada
5. Clique com botão direito no novo botão → **Rename** → `Laser Print & Cut`
6. Opcional: altere o ícone em **Button Appearance**

---

## Passo 6: Usar o Add-in

Com uma imagem selecionada no CorelDRAW:

1. Execute a macro ou clique no botão
2. Na interface que abrir:
   - Ajuste o **Threshold** (limiar de contraste)
   - Ajuste o **Offset** em mm
   - Escolha entre **Only Contour** ou **Print & Cut**
   - Clique em **Accept**

### Modo Only Contour
Gera um vetor de contorno (preto, hairline) sobreposto à imagem.

### Modo Print & Cut (LightBurn)
Cria duas camadas:
- **Print**: bitmap + retângulo envolvente + 2 marcas de registro (vermelho)
- **Cut**: contorno (preto) + mesmas marcas de registro (vermelho)

---

## Solução de problemas

### "Class not registered" (CLSID não registrado)
```cmd
cd "F:\Macros\Print&Cut v2\LaserPrintCutAddin\src\LaserPrintCut\bin\Release\net48"
regasm LaserPrintCut.dll /codebase /verbose
```

### "Permission denied" no registro
Execute `register.bat` ou o CMD como **Administrador**.

### CorelDRAW não encontra o add-in
Verifique no registro se a chave existe:
- `HKCU\Software\Corel\CorelDRAW\25.0\AddIns\LaserPrintCut.Addin`
(Ajuste 25.0 para sua versão: X7=17.0, X8=18.0, 2019=21.0, 2020=22.0, 2023=25.0)

### Nada acontece ao executar a macro
Abra o VBA Editor e pressione **F8** para depurar linha a linha.

### Erro de threading STA
O `Addin.Startup()` já cria uma thread STA. Se houver erro, certifique-se de que o .NET Framework 4.8 está instalado.

---

## Desinstalar

```cmd
cd "F:\Macros\Print&Cut v2\LaserPrintCutAddin"
unregister.bat
```
