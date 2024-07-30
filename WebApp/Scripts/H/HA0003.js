Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/HA0003/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "銀行代碼維護";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '銀行代碼',
                    name: 'P0',
                    id: 'P0',
                    //enforceMaxLength: true, // 限制可輸入最大長度
                    //maxLength: 2, // 可輸入最大長度為2  //20190415  限制輸入資料長度
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '銀行名稱',
                    name: 'P1',
                    id: 'P1',
                    //enforceMaxLength: true, // 限制可輸入最大長度
                    //maxLength: 2, // 可輸入最大長度為2  //20190415  限制輸入資料長度
                    padding: '0 4 0 4'
                },{
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'AGEN_BANK_14', type: 'string' },
            { name: 'BANKNAME', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'AGEN_BANK_14', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/HA0003/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/HA0003/Create'; // HA0003Controller的Create
                    setFormT1('I', '新增');
                    msglabel('');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/HA0003/Update';
                    setFormT1("U", '修改');
                    msglabel('');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/HA0003/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            },
            {
                xtype: 'button',
                itemId: 'import',
                id: 'btnIxport',
                text: '匯入', 
                handler: function () {
                    msglabel('訊息區:');
                    showWin6();
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model'); // /Scripts/app/model/MI_MATCLASS.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("AGEN_BANK_14"); // 物料分類代碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
        }
        else {
            u = f.findField('BANKNAME');
        }
        f.findField('x').setValue(x);
        f.findField('BANKNAME').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]    
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]    
        }],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "銀行代碼",
                dataIndex: 'AGEN_BANK_14',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "銀行名稱",
                dataIndex: 'BANKNAME',
                width: 400
            }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('BANKNAME');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T1Form.getForm().reset();
        }
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '銀行代碼',
            name: 'AGEN_BANK_14',
            enforceMaxLength: true,
            maxLength: 7,           //20190415  限制輸入資料長度
            regexText: '只能輸入數字',
            regex: /^[0-9]*$/, // 用正規表示式限制可輸入內容
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required',
        }, {
            xtype: 'textarea',      //20190415  根據測試結果要求修改，改為multiline的Textbox
            fieldLabel: '銀行名稱',
            name: 'BANKNAME',
            enforceMaxLength: false,
            maxLength: 64,          //20190415  限制輸入資料長度
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required'
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
                else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1Query.getForm().findField('P0').setValue(f2.findField('AGEN_BANK_14').getValue());
                            T1Query.getForm().findField('P1').setValue('');
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            T1Query.getForm().findField('P0').setValue(f2.findField('AGEN_BANK_14').getValue());
                            T1Query.getForm().findField('P1').setValue('');
                            T1Load();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Query.getForm().findField('P0').setValue('');
                            T1Query.getForm().findField('P1').setValue('');
                            T1Load();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            msglabel('訊息區:Form fields may not be submitted with invalid values');
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            msglabel('訊息區:Ajax communication failed');
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            msglabel('訊息區:' + action.result.msg);
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

 var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }]
    });

 //匯入功能
 var winActWidth = viewport.width - 20;
 var winActHeight = viewport.height - 20;
 var T6Query = Ext.widget({
     xtype: 'form',
     layout: 'hbox',
     defaultType: 'textfield',
     fieldDefaults: {
         labelWidth: 80
     },
     border: false,
     items: [
         {
             xtype: 'button',
             id: 'T6sample',
             name: 'T6sample',
             text: '範本', handler: function () {
                 var p = new Array();
                 PostForm('../../api/HA0003/GetExcelExample', p);
                 msglabel('訊息區:匯出完成');
             }
         },
         {
             xtype: 'filefield',
             name: 'T6send',
             id: 'T6send',
             buttonOnly: true,
             buttonText: '匯入',
             width: 40,
             listeners: {
                 change: function (widget, value, eOpts) {
                     //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                     Ext.getCmp('T6insert').setDisabled(true);
                     T6Store.removeAll();
                     var files = event.target.files;
                     var self = this; // the controller
                     if (!files || files.length == 0) return; // make sure we got something
                     var f = files[0];
                     var ext = this.value.split('.').pop();
                     if (!/^(xls|xlsx)$/.test(ext)) {
                         Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                         Ext.getCmp('T6send').fileInputEl.dom.value = '';
                         msglabel("請選擇xlsx或xls檔案！");
                     }
                     else {
                         var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                         myMask.show();
                         var formData = new FormData();
                         formData.append("file", f);
                         var ajaxRequest = $.ajax({
                             type: "POST",
                             url: "/api/HA0003/SendExcel",
                             data: formData,
                             processData: false,
                             //必須false才會自動加上正確的Content-Type
                             contentType: false,
                             success: function (data, textStatus, jqXHR) {
                                 if (!data.success) {
                                     T6Store.removeAll();
                                     Ext.MessageBox.alert("提示", data.msg);
                                     msglabel("訊息區:");
                                     Ext.getCmp('T6insert').setDisabled(true);
                                     IsSend = false;
                                 }
                                 else {
                                     msglabel("訊息區:檔案讀取成功");
                                     T6Store.loadData(data.etts, false);
                                     IsSend = true;
                                     T6Grid.columns[1].setVisible(true);
                                     //T1Grid.columns[2].setVisible(true);
                                     if (data.msg == "True") {
                                         Ext.getCmp('T6insert').setDisabled(false);
                                         Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");
                                     };
                                     if (data.msg == "False") {
                                         Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                     };
                                 }
                                 Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                 myMask.hide();
                             },
                             error: function (jqXHR, textStatus, errorThrown) {
                                 Ext.Msg.alert('失敗', 'Ajax communication failed');
                                 Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                 Ext.getCmp('T6insert').setDisabled(true);
                                 myMask.hide();

                             }
                         });
                     }
                 }
             }
         },
         {
             xtype: 'button',
             text: '更新',
             id: 'T6insert',
             name: 'T6insert',
             disabled: true,
             handler: function () {
                 var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                 myMask.show();
                 Ext.Ajax.request({
                     url: '/api/HA0003/InsertFromXls',
                     method: reqVal_p,
                     params: {
                         data: Ext.encode(Ext.pluck(T6Store.data.items, 'data'))
                     },
                     success: function (response) {
                         var data = Ext.decode(response.responseText);
                         if (data.success) {
                             if (data.msg == "True") {
                                 Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                 msglabel("訊息區:<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                             }
                             else {
                                 Ext.MessageBox.alert("提示", "匯入<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                 msglabel("訊息區:匯入<span style=\"color: red; font-weight: bold\">完成</span>");
                             }
                             Ext.getCmp('T6insert').setDisabled(true);
                             T6Store.removeAll();
                             T6Grid.columns[1].setVisible(false);
                             T1Load(true, true);
                         }
                         myMask.hide();
                         Ext.getCmp('T6insert').setDisabled(true);
                     },
                     failure: function (form, action) {
                         myMask.hide();
                         Ext.getCmp('T6insert').setDisabled(true);
                         switch (action.failureType) {
                             case Ext.form.action.Action.CLIENT_INVALID:
                                 Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                 break;
                             case Ext.form.action.Action.CONNECT_FAILURE:
                                 Ext.Msg.alert('失敗', 'Ajax communication failed');
                                 break;
                             case Ext.form.action.Action.SERVER_INVALID:
                                 Ext.Msg.alert('失敗', "匯入失敗");
                                 break;
                         }
                     }
                 });

                 hideWin6();
             }
         }
     ]
 });
 var T6Store = Ext.create('Ext.data.Store', {
     model: 'T1Model',
     pageSize: 1000, // 每頁顯示筆數
     remoteSort: true,
     sorters: [{ property: 'AGEN_BANK_14', direction: 'ASC' }],
     proxy: {
         type: 'ajax',
         actionMethods: {
             read: 'POST' // by default GET
         },
         //url: '/api/AA0159/???',
         reader: {
             type: 'json',
             rootProperty: 'etts',
             totalProperty: 'rc'
         }
     },
     listeners: {
         beforeload: function (store, options) {
             store.removeAll();
             var np = {
                 //p2: T1F2,
                 //p3: T1F5
             };
             Ext.apply(store.proxy.extraParams, np);
         }
     }
 })
 var T6Grid = Ext.create('Ext.grid.Panel', {
     autoScroll: true,
     store: T6Store,
     plain: true,
     loadingText: '處理中...',
     loadMask: true,
     cls: 'T2',
     dockedItems: [
         {
             dock: 'top',
             xtype: 'toolbar',
             items: [T6Query]
         }
     ],
     columns: [
         { xtype: 'rownumberer' },
         {
             text: "檢核結果",
             dataIndex: 'CHECK_RESULT',
             hidden: true,
             width: 150
         },
         { text: "銀行代碼", dataIndex: 'AGEN_BANK_14', width: 100 },
         { text: "銀行名稱", dataIndex: "BANKNAME", width: 250 },
         { header: "", flex: 1 }
     ]
 });
 var win6;
 if (!win6) {
     win6 = Ext.widget('window', {
         title: '匯入',
         closeAction: 'hide',
         width: winActWidth,
         height: winActHeight,
         layout: 'fit',
         resizable: true,
         modal: true,
         constrain: true,
         items: [T6Grid],
         buttons: [{
             text: '關閉',
             handler: function () {
                 hideWin6();
             }
         }],
         listeners: {
             move: function (xwin, x, y, eOpts) {
                 xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                 xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
             },
             resize: function (xwin, width, height) {
                 winActWidth = width;
                 winActHeight = height;
             }
         }
     });
 }

 function showWin6() {
     if (win6.hidden) {
         T6Cleanup();
         T6Store.removeAll();
         win6.show();
     }
 }
 function hideWin6() {
     if (!win6.hidden) {
         win6.hide();
         T6Cleanup();
     }
 }

 function T6Cleanup() {
     T6Query.getForm().reset();
     msglabel('訊息區:');
 }

    T1Load(); // 進入畫面時自動載入一次資料   
    T1Query.getForm().findField('P0').focus();
});
