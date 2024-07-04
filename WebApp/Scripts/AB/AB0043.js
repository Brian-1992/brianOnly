Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
 
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var viewModel = Ext.create('WEBAPP.store.AB.AB0043VM');   // 定義於/Scripts/app/store/AB/AB0043VM.js

    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "化療調劑時點設定"; 
    var T1Rec = 0;
    var T1LastRec = null;

    var oldp0 = '';
    var oldp1 = '';
    var oldp2 = '';
    var oldp3 = '';

    var newp0 = '';
    var newp1 = '';
    var newp2 = '';
    var newp3 = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 查詢欄位
    //var T1Query = Ext.widget({
    //    xtype: 'form',
    //    layout: 'hbox',
    //    border: false,
    //    fieldDefaults: {
    //        xtype: 'textfield',
    //        labelAlign: 'right',
    //        labelWidth: 95
    //    },
    //    items: [
    //        {
    //            //xtype: 'textfield',
    //            xtype: 'combobox',
    //            fieldLabel: '調劑類別',
    //            name: 'P0',
    //            id: 'P0',
    //            enforceMaxLength: true, // 限制可輸入最大長度
    //            //maxLength: 8, // 可輸入最大長度為8
    //            displayField: 'RxTypeName',
    //            valueField: 'RxTypeID',
    //            labelWidth: 55,
    //            selectIndex: 0,
    //            padding: '0 4 0 4',
    //            store: [
    //                { RxTypeName: '', RxTypeID: '', default: 0 },
    //                { RxTypeName: 'CHEMO', RxTypeID: '0' },
    //                { RxTypeName: 'TPN', RxTypeID: '1' }
    //            ]
    //        }, {
    //              xtype: 'combobox',
    //              fieldLabel: '調劑日期類別',
    //              name: 'P1',
    //              id: 'P1',
    //              enforceMaxLength: true, // 限制可輸入最大長度
    //              //maxLength: 8, // 可輸入最大長度為8
    //              displayField: 'RxDateKindName',
    //              valueField: 'RxDataKindID',
    //              labelWidth: 80,
    //              selectIndex: 0,
    //              padding: '0 4 0 4',
    //              store: [
    //                  { RxDateKindName: '', RxDataKindID: '', default: 0},
    //                  { RxDateKindName: '工作日', RxDataKindID: '0' },
    //                  { RxDateKindName: '星期六', RxDataKindID: '1' },
    //                  { RxDateKindName: '星期日及國定假日', RxDataKindID: '2' },
    //                  { RxDateKindName: '特定假日', RxDataKindID: '3' }
    //              ]
    //        },  {
    //            xtype: 'button',
    //            text: '查詢',
    //            handler: function () {
    //                T1Load();
    //                msglabel('訊息區:');
    //            }
    //        }, {
    //            xtype: 'button',
    //            text: '清除',
    //            handler: function () {
    //                var f = this.up('form').getForm();
    //                f.reset();
    //                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
    //                msglabel('訊息區:');
    //            }
    //        }
    //    ]
    //});


    var T1Store = viewModel.getStore('PHRSDPT');              // 定義於/Scripts/app/model/PHRSDPT.js 

    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        viewport.down('#form').collapse();
        T1Store.getProxy().setExtraParam("p0", "");
        T1Store.load({
            params: {
                start: 0
            },
            callback: function (records, operation, success) {
                if (success) {
                    //Ext.getCmp('btnQry2').setDisabled(false);     // Detail Qry button enable
                    //if (records.length == 0)
                    //    Ext.Msg.alert('沒有符合的資料!.......')
                    //else if (records.length > 0)
                    //    T1Tool.setDisabled(false);
                } else {
                    //Ext.getCmp('btnQry2').setDisabled(true);
                    //Ext.Msg.alert('訊息', 'failure!');
                }
            }
        })

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
                    T1Set = '/api/AB0043/Create'; // AA0043Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0043/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0043/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        //viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            var r = Ext.create('WEBAPP.model.PHRSDPT'); // /Scripts/app/model/PHRSDPT.js
            T1Form.loadRecord(r);       // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("RXTYPE");  // 調劑類別在新增時才可填寫
            f.findField('RXTYPE').setReadOnly(false);        // 調劑類別
            f.findField('RXDATEKIND').setReadOnly(false);    // 調劑日期類別
            f.findField('DEADLINETIME').setReadOnly(false);  // 調劑時點
            f.findField('WORKFLAG').setReadOnly(false);      // 是否調劑
            u.clearInvalid();
            f.findField('WORKFLAG').setValue('N'); // 是否調劑預設為N
        }
        else {
            u = f.findField('RXTYPE');
            f.findField('RXTYPE').setReadOnly(false);        // 調劑類別
            f.findField('RXDATEKIND').setReadOnly(false);    // 調劑日期類別
            f.findField('DEADLINETIME').setReadOnly(false); // 調劑時點
            f.findField('WORKFLAG').setReadOnly(false);     // 是否調劑
        }

        f.findField('x').setValue(x);
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
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    layout: 'fit',
            //    items: [T1Query]
            //    },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "調劑類別",
            dataIndex: 'RXTYPE',
            width: 150,
            renderer: function (value) {
                if (value == '1')
                    return "CHEMO";
                else if (value == '2')
                    return "TPN";
            }
        }, {
            text: "調劑日期類別",
            dataIndex: 'RXDATEKIND',
            width: 180,
            renderer: function (value) {
                if (value == '1')
                    return "工作日";
                else if (value == '2')
                    return "星期六";
                else if (value == '3')
                    return "星期日及國定假日";
                else if (value == '4')
                    return "特定假日";
            }
        }, {
            text: "調劑時點",
            dataIndex: 'DEADLINETIME',
            width: 160
        }, {
            text: "是否調劑",
            dataIndex: 'WORKFLAG',
            width: 170,
            renderer: function (value) {
                if (value == 'Y')
                    return "是";
                else if (value == 'N')
                    return "否";
            }
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
            var u = f.findField('RXTYPE');
            oldp0 = f.findField('RXTYPE').getValue();
            oldp1 = f.findField('RXDATEKIND').getValue();
            oldp2 = f.findField('DEADLINETIME').getValue();
            oldp3 = f.findField('WORKFLAG').getValue();
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('RXTYPE').setReadOnly(true);
            f.findField('RXDATEKIND').setReadOnly(true);
            f.findField('DEADLINETIME').setReadOnly(true);
            f.findField('WORKFLAG').setReadOnly(true);
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
            xtype: 'combo',
            fieldLabel: '調劑類別',
            name: 'RXTYPE',           // 與 PHRSDPT 欄位同名
            id: 'RXTYPE_ID',
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            editable: false,
            forceSelection: true,
            readOnly: true,
            fieldCls: 'required',
            store: [
                { TEXT: 'CHEMO', VALUE: '1' },
                { TEXT: 'TPN', VALUE: '2' }
            ]
        }, {
            xtype: 'combo',
            fieldLabel: '調劑日期類別',
            name: 'RXDATEKIND',          // 與 PHRSDPT 欄位同名   
            id: 'RXDATEKIND_ID',
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            editable: false,
            forceSelection: true,
            readOnly: true,
            fieldCls: 'required',
            store: [
                { TEXT: '工作日', VALUE: '1' },
                { TEXT: '星期六', VALUE: '2' },
                { TEXT: '星期日及國定假日', VALUE: '3' },
                { TEXT: '特定假日', VALUE: '4' },
            ]
        }, {
            ////////////xtype: 'timefield',
            ////////////name: 'DEADLINETIME',
            ////////////fieldLabel: '調劑時點',
            ////////////format: 'H:i',
            ////////////minValue: '00:00',
            ////////////maxValue: '23:45',

            xtype: 'textfield',
            fieldLabel: '調劑時點',
            name: 'DEADLINETIME',
            id: 'DEADLINETIME_ID',
            enforceMaxLength: true,
            maxLength: 40,
            regexText: '只能輸入4位數字',
            regex: /^\d{4}$/,        // 用正規表示式限制只可輸入4個數字
            readOnly: true,
            fieldCls: 'required'
        }
            , {
            xtype: 'combo',
            fieldLabel: '是否調劑',
            name: 'WORKFLAG',       // 與 PHRSDPT 欄位同名
            id: 'WORKFLAG_ID',
            displayField: 'TEXT',
            valueField: 'VALUE',
            editable: false,
            forceSelection: true,
            readOnly: true,
            fieldCls: 'required',
            store: [
                { TEXT: '是', VALUE: 'Y' },
                { TEXT: '否', VALUE: 'N' }
            ]
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if ((this.up('form').getForm().findField('RXTYPE').getValue() == null) || (this.up('form').getForm().findField('RXDATEKIND').getValue() == null)
                        || (this.up('form').getForm().findField('DEADLINETIME').getValue() == ''
                        ))
                        Ext.Msg.alert('提醒', '至少需輸入');
                    else {
                        var deadlinetime = parseInt(this.up('form').getForm().findField('DEADLINETIME').getValue());
                        if (deadlinetime < 0 || deadlinetime > 2359) {
                            Ext.Msg.alert('提醒', '調劑時間至少需輸入0000-2359 之間!');
                            msglabel('訊息區:調劑時間至少需輸入 0000-2359 之間!');
                        } else {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Submit();
                                }
                            });
                        }
                    }
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
                params: {
                           p0: oldp0,
                           p1: oldp1,
                           p2: oldp2,
                           p3: oldp3,
                           newp0 : Ext.getCmp('RXTYPE_ID').getValue(),
                           newp1 : Ext.getCmp('RXDATEKIND_ID').getValue(),
                           newp2 : Ext.getCmp('DEADLINETIME_ID').getValue(),
                           newp3 : Ext.getCmp('WORKFLAG_ID').getValue()
                },
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
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Store.load();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            T1Store.load();
                            msglabel('訊息區:資料刪除成功');
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
        viewport.down('#form').setTitle('瀏覽');
        viewport.down('#form').collapse();
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
        }
        ]
    });

    T1Load(); // 進入畫面時自動載入一次資料

});
