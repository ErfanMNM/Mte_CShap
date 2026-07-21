import React, { useState, useRef } from "react";
import Keyboard from "react-simple-keyboard";
import "react-simple-keyboard/build/css/index.css";

type KeyboardLayoutType = "default" | "shift" | "numeric";

interface KeyboardContextType {
  openKeyboard: (
    initialValue: string,
    layout: KeyboardLayoutType,
    onChange: (val: string) => void,
  ) => void;
  closeKeyboard: () => void;
  isOpen: boolean;
}

const KeyboardContext = React.createContext<KeyboardContextType | null>(null);

export const useVirtualKeyboard = () => {
  const ctx = React.useContext(KeyboardContext);
  if (!ctx)
    throw new Error(
      "useVirtualKeyboard must be used within KeyboardProvider",
    );
  return ctx;
};

const CloseIcon = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width="16"
    height="16"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="2.5"
    strokeLinecap="round"
    strokeLinejoin="round"
  >
    <line x1="18" y1="6" x2="6" y2="18" />
    <line x1="6" y1="6" x2="18" y2="18" />
  </svg>
);

export const KeyboardProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [value, setValue] = useState("");
  const [layout, setLayout] = useState<KeyboardLayoutType>("default");
  const [onChangeRef, setOnChangeRef] = useState<{ fn: (val: string) => void }>(
    { fn: () => {} },
  );
  const keyboardRef = useRef<any>(null);

  const openKeyboard = (
    initialValue: string,
    layoutType: KeyboardLayoutType,
    onChange: (val: string) => void,
  ) => {
    setValue(initialValue);
    setLayout(layoutType);
    setOnChangeRef({ fn: onChange });
    setIsOpen(true);
    if (keyboardRef.current) {
      keyboardRef.current.setInput(initialValue);
    }
  };

  const closeKeyboard = () => {
    setIsOpen(false);
  };

  const onChange = (input: string) => {
    setValue(input);
    onChangeRef.fn(input);
  };

  const handleShift = () => {
    const currentLayout = layout;
    let nextLayout = currentLayout;
    if (currentLayout === "default") nextLayout = "shift";
    else if (currentLayout === "shift") nextLayout = "default";

    if (nextLayout !== currentLayout) {
      setLayout(nextLayout as KeyboardLayoutType);
    }
  };

  const onKeyPress = (button: string) => {
    if (button === "{shift}" || button === "{lock}") handleShift();
    if (button === "{enter}" || button === "{escape}") {
      closeKeyboard();
    }
  };

  return (
    <KeyboardContext.Provider value={{ openKeyboard, closeKeyboard, isOpen }}>
      {children}
      {isOpen && (
        <div className="fixed bottom-0 left-0 right-0 z-[100] p-3 md:p-4 bg-[#d1d5db]/90 backdrop-blur-md border-t border-slate-300 shadow-xl animate-in slide-in-from-bottom-full duration-300">
          <div className="max-w-4xl mx-auto relative">
            <div className="absolute -top-[60px] left-0 right-0 flex justify-between items-center bg-white p-2 px-4 rounded-t-xl md:rounded-xl shadow-lg border border-slate-200 gap-4">
              <input
                type={layout === "numeric" ? "number" : "text"}
                value={value}
                onChange={(e) => {
                  onChange(e.target.value);
                  keyboardRef.current?.setInput(e.target.value);
                }}
                className="flex-1 text-lg font-medium text-slate-800 bg-transparent outline-none p-1"
                placeholder="Nhập giá trị..."
                autoFocus
              />
              <button
                onClick={closeKeyboard}
                className="bg-slate-100 text-slate-500 p-2 rounded-lg hover:bg-red-50 hover:text-red-600 transition-colors flex items-center justify-center shrink-0"
              >
                <CloseIcon />
              </button>
            </div>

            <style>{`
              .hg-theme-default {
                background-color: transparent !important;
                border-radius: 0 !important;
                padding: 0 !important;
              }
              .hg-button {
                background: white !important;
                border-radius: 8px !important;
                box-shadow: 0 1px 1px rgba(0,0,0,0.2) !important;
                color: #0f172a !important;
                font-weight: 500 !important;
                border-bottom: 1px solid #d1d5db !important;
              }
              .hg-button:active {
                background: #e2e8f0 !important;
                transform: translateY(1px) !important;
                box-shadow: none !important;
                border-bottom: none !important;
              }
              .hg-button.hg-standardBtn.hg-button-enter {
                background: #3b82f6 !important;
                color: white !important;
                font-weight: 600 !important;
                border-bottom: 1px solid #2563eb !important;
              }
              .hg-button.hg-standardBtn.hg-button-enter:active {
                background: #2563eb !important;
              }
              .hg-button.hg-standardBtn.hg-button-shift,
              .hg-button.hg-standardBtn.hg-button-bksp {
                background: #cbd5e1 !important;
                border-bottom: 1px solid #94a3b8 !important;
                color: #1e293b !important;
              }
              .hg-button.hg-standardBtn.hg-button-space {
                background: white !important;
              }
              .hg-row {
                margin-bottom: 8px !important;
              }
              .hg-row:last-child {
                margin-bottom: 0 !important;
              }
            `}</style>

            <div className="p-1 sm:p-2 w-full">
              <Keyboard
                keyboardRef={(r) => (keyboardRef.current = r)}
                layoutName={layout}
                onChange={onChange}
                onKeyPress={onKeyPress}
                display={{
                  "{bksp}": "⌫",
                  "{enter}": "Xong",
                  "{shift}": "⇧",
                  "{space}": "Dấu cách",
                  "{tab}": "Tab",
                  "{lock}": "Caps",
                  "{escape}": "Đóng",
                }}
                layout={{
                  default: [
                    "q w e r t y u i o p",
                    "a s d f g h j k l",
                    "{shift} z x c v b n m {bksp}",
                    "{space} {enter}",
                  ],
                  shift: [
                    "Q W E R T Y U I O P",
                    "A S D F G H J K L",
                    "{shift} Z X C V B N M {bksp}",
                    "{space} {enter}",
                  ],
                  numeric: [
                    "1 2 3 4 5 6 7 8 9 0",
                    "- / : ; ( ) $ & @ \"",
                    "{bksp} . , ? ! ' {enter}",
                    "{space} {escape}",
                  ],
                }}
              />
            </div>
          </div>
        </div>
      )}
    </KeyboardContext.Provider>
  );
};

export default KeyboardProvider;
