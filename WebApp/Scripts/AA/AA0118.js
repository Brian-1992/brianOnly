Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/AA0118/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "物料分類維護";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isFA = Ext.getUrlParam('isFA');

    var form_recStatus = Ext.create('Ext.data.Store', {
        fields: ['KEYCODE', 'VALUE'],
        data: [
            { "KEYCODE": "A", "NAME": "啟用", "COMBITEM": "A:啟用" },
            { "KEYCODE": "X", "NAME": "停用", "COMBITEM": "X:停用" }
        ]
    });

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
                    fieldLabel: '物料分類代碼',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 2, // 可輸入最大長度為2  //20190415  限制輸入資料長度
                    padding: '0 4 0 4'
                }, {
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

    var T1Store = Ext.create('WEBAPP.store.MI_MATCLASS', { // 定義於/Scripts/app/store/MI_MATCLASS.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
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
                    T1Set = '/api/AA0118/Create'; // AA0118Controller的Create
                    setFormT1('I', '新增');
                    msglabel('訊息區:');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0118/Update';
                    setFormT1("U", '修改');
                    msglabel('訊息區:');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0118/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            },
            {
                itemId: 'export', text: '匯出',
                id: 'export',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '物料分類清單.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    PostForm('/api/AA0118/Excel', p);
                    msglabel('匯出完成');
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
            var r = Ext.create('WEBAPP.model.MI_MATCLASS'); // /Scripts/app/model/MI_MATCLASS.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("MAT_CLASS"); // 物料分類代碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
        }
        else {
            u = f.findField('MAT_CLSNAME');
        }
        f.findField('x').setValue(x);
        f.findField('MAT_CLSNAME').setReadOnly(false);
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
            items: [T1Query]    //20190415  交換查詢列與工具列位置
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]     //20190415  交換查詢列與工具列位置
        }],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "物料分類代碼",
                dataIndex: 'MAT_CLASS',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "物料分類名稱",
                dataIndex: 'MAT_CLSNAME',
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
            var u = f.findField('MAT_CLASS');
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
            fieldLabel: '物料分類代碼',
            name: 'MAT_CLASS',
            enforceMaxLength: true,
            maxLength: 2,           //20190415  限制輸入資料長度
            regexText: '只能輸入數字',
            regex: /^\d{2}$/, // 用正規表示式限制可輸入內容
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required',
        }, {
            xtype: 'textarea',      //20190415  根據測試結果要求修改，改為multiline的Textbox
            fieldLabel: '物料分類名稱',
            name: 'MAT_CLSNAME',
            enforceMaxLength: true,
            maxLength: 30,          //20190415  限制輸入資料長度
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
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
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
            fc.setReadOnly(true);
            fc.readOnly = true;
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

    //T1Load(); // 進入畫面時自動載入一次資料    //一開始不要查詢
    T1Query.getForm().findField('P0').focus();
    Ext.getCmp('export').hide();
    if (isFA == 'Y') {
        Ext.getCmp('export').show();
    }
});
