Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AA0039/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1GetExcel = '../../../api/AA0140/Excel';
    var T1Name = "藥品院內自辦購案基本檔維護";

    var T1Rec = 0;
    var T1LastRec = null;

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [            
            { name: 'MMCODE', type: 'string' },                // 院內碼
            { name: 'SELF_CONT_BDATE', type: 'string' },       // 藥品契約生效起日
            { name: 'SELF_CONT_EDATE', type: 'string' },       // 藥品契約生效迄日  
            { name: 'SELF_CONTRACT_NO', type: 'string' },      // 合約案號
            { name: 'SELF_PUR_UPPER_LIMIT', type: 'string' },  // 採購上限金額
            { name: 'SELF_CONT_BDATE_virtual', type: 'string' },// 藥品契約生效起日(虛擬) 
            { name: 'SELF_CONT_EDATE_virtual', type: 'string' },// 藥品契約生效迄日(虛擬)
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SELF_CONT_BDATE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0140/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    mmcode: T1Query.getForm().findField('MMCODE').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('btnExcel').setDisabled(false);                    
                } else {
                    Ext.getCmp('btnExcel').setDisabled(true);
                }
            }
        }
    });

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 95
        },
        items: [
            {
                xtype: 'textfield',
                fieldLabel: '院內碼',
                name: 'MMCODE',
                id: 'MMCODE',
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 8, // 可輸入最大長度為8
                padding: '0 4 0 4'
            }, {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    T1Load(true);
                    msglabel('');
                }
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('MMCODE').focus(); // 進入畫面時輸入游標預設在MMCODE
                    msglabel('');
                }
            }
        ]
    });

    function T1Load(moveFirst) {
        if (moveFirst) {
            T1Tool.moveFirst(); //移动到第一页，与单击“first”按钮有相同的效果。
        } else {
            T1Store.load({
                params: {
                    start: 0 //start: 0 從第0筆開始顯示,如果要從後端控制每個分頁起始, 可從這邊傳給後端
                }
            });
        }
        
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true, //列示資料筆數
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/AA0140/Create'; // AA0140Controller的Create
                    msglabel('');
                    setFormT1('I', '新增');
                }
            },{
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0140/Update';
                    msglabel('');
                    setFormT1("U", '修改');
                }
            },{
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0140/Delete';
                            T1Form.getForm().findField('x').setValue('D'); //設定x的值為D
                            T1Submit();
                        }
                    }
                    );
                }
            }, {
                text: '匯出',
                id: 'btnExcel',
                name: 'btnExcel',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '藥品院內自辦購案基本檔.xls' });
                    p.push({ name: 'MMCODE', value: T1Query.getForm().findField('MMCODE').getValue() }); 
                    PostForm(T1GetExcel, p);
                    msglabel('匯出完成');
                }
            }, {
                xtype: 'filefield',
                name: 'uploadExcel',
                id: 'uploadExcel',
                buttonText: '匯入',
                buttonOnly: true,
                padding: '0 4 0 0',
                width: 72,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files; //取得檔案訊息
                        if (!files || files.length == 0) return; // make sure we got something
                        var file = files[0];
                        var ext = this.value.split('.').pop();//取得副檔名
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            Ext.getCmp('import').fileInputEl.dom.value = '';
                            msglabel('');
                        } else {
                            uploadCheckYWindow.show();
                            msglabel("已選擇檔案");
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", file);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0140/Upload",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    myMask.hide();
                                    if (!data.success) {
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';
                                    }
                                    else {
                                        msglabel("訊息區:資料匯入成功");
                                        Ext.getCmp('uploadExcel').fileInputEl.dom.value = '';

                                        //T1Load();
                                        setT2Store(data.etts);
                                    }
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    myMask.hide();
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('uploadExcel').setRawValue("");
                                }
                            });
                        }
                    }
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model');
            
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("MMCODE"); // 院內碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
        }
        else {
            u = f.findField('MMCODE');
        }
        f.findField('x').setValue(x);
        f.findField('SELF_CONT_BDATE').setReadOnly(false);
        f.findField('SELF_CONT_EDATE').setReadOnly(false);
        f.findField('SELF_CONTRACT_NO').setReadOnly(false);
        f.findField('SELF_PUR_UPPER_LIMIT').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 130
        }, {
            text: "藥品契約生效起日",
            dataIndex: 'SELF_CONT_BDATE',
            width: 120
        }, {
            text: "藥品契約生效迄日",
            dataIndex: 'SELF_CONT_EDATE',
            width: 120
        }, {
            text: "合約案號",
            dataIndex: 'SELF_CONTRACT_NO',
            width: 120
        }, {
            text: "採購上限金額",
            dataIndex: 'SELF_PUR_UPPER_LIMIT',
            width: 120
        }, {
            header: "",
            flex: 1
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
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('MMCODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('SELF_CONT_BDATE').setReadOnly(true);
            f.findField('SELF_CONT_EDATE').setReadOnly(true);
            f.findField('SELF_CONTRACT_NO').setReadOnly(true);
            f.findField('SELF_PUR_UPPER_LIMIT').setReadOnly(true);
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
        items: [
            {
                name: 'x',
                xtype: 'hidden'
            }, Ext.create('WEBAPP.form.MMCodeCombo', {
                name: 'MMCODE',
                fieldLabel: '院內碼',
                emptyText: '全部',
                width: 200,
                limit: 10,
                queryUrl: '/api/AA0140/GetMMCodeCombo',
                extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                readOnly: true,
                getDefaultParams: function () {

                },
                listeners: {
                }
            }), {
                xtype: 'datefield',
                fieldLabel: '藥品契約生效起日',
                name: 'SELF_CONT_BDATE',
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                readOnly: true,
            }, {
                xtype: 'datefield',
                fieldLabel: '藥品契約生效迄日',
                name: 'SELF_CONT_EDATE',
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                readOnly: true,
            }, {
                fieldLabel: '合約案號',
                name: 'SELF_CONTRACT_NO',
                enforceMaxLength: true,
                maxLength: 20,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required'
            }, {
                fieldLabel: '採購上限金額',
                name: 'SELF_PUR_UPPER_LIMIT',
                enforceMaxLength: true,
                maxLength: 20,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required'
            },{
                xtype: 'hidden',
                fieldLabel: '藥品契約生效起日(虛擬)',
                name: 'SELF_CONT_BDATE_virtual',    
                readOnly: true,
                submitValue:true
            }, {
                xtype: 'hidden',
                fieldLabel: '藥品契約生效迄日(虛擬)',
                name: 'SELF_CONT_EDATE_virtual',
                readOnly: true,
                submitValue: true
            }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
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
                    //T1Load();
                    var f2 = T1Form.getForm();
                    //var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //var v = action.result.etts[0];
                            //r.set(v);
                            //T1Store.insert(0, r);
                            //r.commit();
                            T1Load(false);
                            msglabel('資料新增成功');
                            break;
                        case "U":
                            //var v = action.result.etts[0];
                            //r.set(v);
                            //r.commit();
                            T1Load(false);
                            msglabel('資料修改成功');
                            break;     
                        case "D":        
                            T1Load(false);
                            msglabel('資料刪除成功');
                            break;                              
                    }
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
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
        //viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }


    //#region uploadCheckYWindow 匯入時檢查資料是否全符合，彈出視窗顯示結果
    function setT2Store(datas) {
        
        T2Store.removeAll();
        var showConfirm = true;
        for (var i = 0; i < datas.length; i++) {
            T2Store.add(datas[i]);
            if (datas[i].SaveStatus == 'N') {
                showConfirm = false;
            }
        }

        if (showConfirm == false) {
            Ext.getCmp('uploadConfirmY').hide();
        } else {
            Ext.getCmp('uploadConfirmY').show();
        }
    }

    var T2Store = Ext.create('Ext.data.Store', {
        fields: ['MMCODE', 'SaveStatus', 'UploadMsg', 'SELF_CONT_BDATE', 'SELF_CONT_EDATE',
            'SELF_CONTRACT_NO', 'SELF_PUR_UPPER_LIMIT']
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        id: 't2Grid',
        columns: [{
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "是否通過",
            dataIndex: 'SaveStatus',
            width: 80,
            renderer: function (val, meta, record) {
                if (record.data.SaveStatus == 'Y') {
                    return '通過';
                }
                if (record.data.SaveStatus == 'N') {
                    return '不通過';
                }
            },
        }, {
            text: "原因",
            dataIndex: 'UploadMsg',
            width: 120,
            renderer: function (value, meta, record) {
                meta.style = 'white-space:normal;word-break:break-all;';
                return value;
            }
        }, {
            text: "藥品契約生效起日",
            dataIndex: 'SELF_CONT_BDATE',
            width: 150
        }, {
            text: "藥品契約生效迄日",
            dataIndex: 'SELF_CONT_EDATE',
            width: 150
        }, {
            text: "合約案號",
            dataIndex: 'SELF_CONTRACT_NO',
            width: 120
        }, {
            text: "採購上限金額",
            dataIndex: 'SELF_PUR_UPPER_LIMIT',
            width: 120
        }, {
            header: "",
            flex: 1
        }],
    });

    var uploadCheckYWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'uploadCheckYWindow',
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        modal: true, //true 掩閉背后的一切
        title: '本次修改項目(有填欄位才會更新)',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't2Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T2Grid]
        }],
        listeners: {
            show: function () {
                uploadCheckYWindow.center();
            }
        },
        buttons: [
            {
                id: 'uploadConfirmY',
                hidden: true,
                text: '確定上傳',
                handler: function () {
                    
                    var temp_datas = T2Store.getData().getRange();
                    var list = [];
                    for (var i = 0; i < temp_datas.length; i++) {
                        list.push(temp_datas[i].data);
                    }

                    Ext.Ajax.request({
                        url: '/api/AA0140/UploadConfirm',
                        method: reqVal_p,
                        params: { data: Ext.util.JSON.encode(list), isYear: 'Y' },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('資料上傳成功');
                                uploadCheckYWindow.hide();
                                T1Load(true);
                            } else {
                                msglabel('資料上傳失敗');
                                Ext.Msg.alert('提醒', data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            },
            {
                id: 'closeUploadCheckYWindow',
                disabled: false,
                text: '關閉',
                handler: function () {
                    uploadCheckYWindow.hide();
                }
            }
        ]
    });
    //#endregion

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body, //始终渲染在页面
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
            collapsible: false, //是否可以收縮
            title: '',
            border: false,
            items: [T1Grid]
        }, {
            itemId: 'form',
            region: 'east',
            collapsible: true,//是否可以收縮
            floatable: true,
            width: 300,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch' //各子组件的宽度拉伸至与容器的宽度相等.
            },
            items: [T1Form]
        }
        ]
    });    

    T1Load(true); // 進入畫面時自動載入一次資料    
    T1Query.getForm().findField('MMCODE').focus();
});
