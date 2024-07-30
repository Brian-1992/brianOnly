Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec = null;
    
    var T1Store = Ext.create('WEBAPP.store.BA0007', { // 定義於/Scripts/app/store/BA0007.js
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var form_MATCLS = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "1", "TEXT": "藥品", "COMBITEM": "1 藥品" },
            { "VALUE": "2", "TEXT": "衛材", "COMBITEM": "2 衛材" },
            { "VALUE": "3", "TEXT": "一般物品", "COMBITEM": "3 一般物品" }
        ]
    });

    var form_MSTOREID = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "0", "TEXT": "非庫備", "COMBITEM": "0 非庫備" },
            { "VALUE": "1", "TEXT": "庫備", "COMBITEM": "1 庫備" }
        ]
    });

    var form_MTHBAS = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "A", "TEXT": "當月", "COMBITEM": "A 當月" },
            { "VALUE": "B", "TEXT": "次月", "COMBITEM": "B 次月" }
        ]
    });

    // 查詢欄位
    var mLabelWidth = 70;
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
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                var f = T1Query.getForm();
                                if (f.isValid()) {
                                    msglabel('訊息區:');
                                    T1Load();
                                    Ext.getCmp('btnUpdate').setDisabled(true);
                                    Ext.getCmp('btnDel').setDisabled(true);
                                }
                                else {
                                    Ext.MessageBox.alert('提示', '請輸入必填/必選資料後再進行查詢');
                                }
                            }
                        }
                    ]
                }

            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    T1Set = '/api/BA0007/CreateDTBAS';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                disabled: true,
                handler: function () {
                    T1Set = '/api/BA0007/UpdateDTBAS';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    var seq = '';
                    if (selection.length) {
                        $.map(selection, function (item, key) {
                            seq = item.get('MMPRDTBAS_SEQ');
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BA0007/DeleteDTBAS',
                                    params: {
                                        SEQ: seq
                                    },
                                    method: reqVal_p,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');
                                            T1Form.reset();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "物料分類",
                dataIndex: 'MAT_CLSID',
                width: 110,
                renderer: function (value, metaData, record, rowIndex) {
                    if (value == '1')
                        return '藥品';
                    else if (value == '2')
                        return '衛材';
                    else if (value == '3')
                        return '一般物品';
                    else
                        return value;
                }
            },
            {
                text: "庫備識別碼",
                dataIndex: 'M_STOREID',
                width: 110,
                renderer: function (value, metaData, record, rowIndex) {
                    if (value == '0')
                        return '非庫備';
                    else if (value == '1')
                        return '庫備';
                    else
                        return value;
                }
            },
            {
                text: "申購生效起日",
                dataIndex: 'BEGINDATE',
                width: 110
            },
            {
                text: "申購生效迄日",
                dataIndex: 'ENDDATE',
                width: 110
            },
            {
                text: "彙總日",
                dataIndex: 'SUMDATE',
                width: 80
            },
            {
                text: "庫備自哪一天開始抓次月基本檔",
                dataIndex: 'DATEBAS',
                width: 160
            },
            {
                text: "非庫備基本檔抓取當月或次月",
                dataIndex: 'MTHBAS',
                width: 160,
                renderer: function (value, metaData, record, rowIndex) {
                    if (value == 'A')
                        return '當月';
                    else if (value == 'B')
                        return '次月';
                    else
                        return value;
                }
            },
            {
                text: "最後進貨月份",
                dataIndex: 'LASTDELI_MTH',
                width: 110,
                renderer: function (value, metaData, record, rowIndex) {
                    if (value == 'A')
                        return '當月';
                    else if (value == 'B')
                        return '次月';
                }
            },
            {
                text: "最後進貨日",
                dataIndex: 'LASTDELI_DT',
                width: 100
            },
            {
                text: "申購彙總日期流水號",
                dataIndex: 'MMPRDTBAS_SEQ',
                width: 150
            },
            {
                text: "建立時間",
                dataIndex: 'CREATE_TIME',
                width: 140,
            },
            {
                text: "建立人員",
                dataIndex: 'CREATE_USER',
                width: 80,
            },
            {
                text: "異動時間",
                dataIndex: 'UPDATE_TIME',
                width: 140,
            },
            {
                text: "異動人員",
                dataIndex: 'UPDATE_USER',
                width: 80,
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec != null) {
                    setFormT1a();
                }
            }
        }
    });

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x == "I") {
            var r = Ext.create('WEBAPP.model.MM_PR_DTBAS'); // /Scripts/app/model/MM_PR_DTBAS.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            f.clearInvalid();
            f.findField('MMPRDTBAS_SEQ').setValue('系統自編');
            f.findField('LASTDELI_MTH0').setValue(true);
            f.findField('LASTDELI_MTH1').setValue(false);
        }
        f.findField('MAT_CLSID').setReadOnly(false);
        f.findField('M_STOREID').setReadOnly(false);
        f.findField('BEGINDATE').setReadOnly(false);
        f.findField('ENDDATE').setReadOnly(false);
        f.findField('SUMDATE').setReadOnly(false);
        f.findField('DATEBAS').setReadOnly(false);
        f.findField('MTHBAS').setReadOnly(false);
        f.findField('LASTDELI_MTH').setReadOnly(false);
        f.findField('LASTDELI_DT').setReadOnly(false);
        f.findField('x').setValue(x);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            T1Form.loadRecord(T1LastRec);
            setFormCol(T1LastRec.data['M_STOREID']);
            var f = T1Form.getForm();
            if (T1LastRec.data["LASTDELI_MTH"] == 'A') {
                f.findField('LASTDELI_MTH0').setValue(true);
                f.findField('LASTDELI_MTH1').setValue(false);
            }
            else {
                f.findField('LASTDELI_MTH0').setValue(false);
                f.findField('LASTDELI_MTH1').setValue(true);
            }
            
            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
            Ext.getCmp('btnUpdate').setDisabled(false);
            Ext.getCmp('btnDel').setDisabled(false);

            viewport.down('#form').expand();
        }
        else {
            Ext.getCmp('btnUpdate').setDisabled(true);
            Ext.getCmp('btnDel').setDisabled(true);
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
        bodyPadding: '5 5 0 0',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 100
        },
        items: [
            {
                xtype: 'container',
                items: [
                    {
                        fieldLabel: 'Update',
                        name: 'x',
                        xtype: 'hidden'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申購彙總日期流水號',
                        name: 'MMPRDTBAS_SEQ',
                        readOnly: true,
                        submitValue: true,
                    },
                    {
                        xtype: 'combo',
                        store: form_MATCLS,
                        name: 'MAT_CLSID',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        fieldLabel: '物料分類',
                        queryMode: 'local',
                        autoSelect: true,
                        readOnly: true,
                        fieldCls: 'required',
                        allowBlank: false,
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                    },
                    {
                        xtype: 'combo',
                        store: form_MSTOREID,
                        name: 'M_STOREID',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        fieldLabel: '庫備識別碼',
                        queryMode: 'local',
                        autoSelect: true,
                        readOnly: true,
                        fieldCls: 'required',
                        allowBlank: false,
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                        listeners: {
                            select: function (field, record, eOpts) {
                                if (field.getValue() != '' && field.getValue() != null && field.readOnly == false) {
                                    setFormCol(field.getValue());
                                }
                            }
                        }
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '申購生效起日',
                        name: 'BEGINDATE',
                        minValue: 1,
                        maxValue: 28,
                        fieldCls: 'required',
                        allowBlank: false,
                        decimalPrecision: 0,
                        readOnly: true,
                        validator: function (value) {
                            if (value == '0') {
                                return '[申購生效起日]不可輸入"0" !';
                            }
                            return true;
                        }
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '申購生效迄日',
                        name: 'ENDDATE',
                        minValue: 1,
                        maxValue: 28,
                        fieldCls: 'required',
                        allowBlank: false,
                        decimalPrecision: 0,
                        readOnly: true,
                        validator: function (value) {
                            if (value == '0') {
                                return '[申購生效迄日]不可輸入"0" !';
                            }
                            return true;
                        }
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '彙總日',
                        name: 'SUMDATE',
                        minValue: 1,
                        maxValue: 28,
                        fieldCls: 'required',
                        allowBlank: false,
                        decimalPrecision: 0,
                        readOnly: true,
                        validator: function (value) {
                            if (value == '0') {
                                return '[彙總日]不可輸入"0" !';
                            }
                            return true;
                        }
                    },
                    {
                        xtype: 'combo',
                        store: form_MTHBAS,
                        name: 'MTHBAS',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        fieldLabel: '非庫備基本檔抓取當月或次月',
                        queryMode: 'local',
                        autoSelect: true,
                        readOnly: true,
                        fieldCls: 'required',
                        allowBlank: false,
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '庫備自哪一天開始抓次月基本檔',
                        name: 'DATEBAS',
                        minValue: 1,
                        maxValue: 28,
                        fieldCls: 'required',
                        allowBlank: true,
                        hidden: true,
                        decimalPrecision: 0,
                        readOnly: true,
                        validator: function (value) {
                            if (value == '0') {
                                return '[庫備自哪一天開始抓次月基本檔]不可輸入"0" !';
                            }
                            return true;
                        }
                    },
                    {
                        xtype: 'radiogroup',
                        fieldLabel: '最後進貨月份',
                        name: 'LASTDELI_MTH',
                        enforceMaxLength: true,
                        readOnly: true,
                        items: [
                            { boxLabel: '當月', name: 'LASTDELI_MTH', id: 'LASTDELI_MTH0', readOnly: true, inputValue: 'A', width: 70, checked: true },
                            { boxLabel: '次月', name: 'LASTDELI_MTH', id: 'LASTDELI_MTH1', readOnly: true, inputValue: 'B' }
                        ]
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '最後進貨日',
                        labelSeparator: '',
                        name: 'LASTDELI_DT',
                        minValue: 1,
                        maxValue: 28,
                        fieldCls: 'required',
                        allowBlank: false,
                        decimalPrecision: 0,
                        readOnly: true,
                        validator: function (value) {
                            if (value == '0') {
                                return '[最後進貨日]不可輸入"0" !';
                            }
                            return true;
                        }
                    },
                ]
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                params: {
                },
                success: function (form, action) {
                    var f2 = T1Form.getForm();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel("新增成功");
                            break;
                        case "U":
                            msglabel("資料更新完成");
                            break;
                    }
                    T1Cleanup();
                    T1Load();
                    myMask.hide();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', '資料輸入錯誤');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', '網頁連線失敗');
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
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        f.findField('MAT_CLSID').setReadOnly(true);
        f.findField('M_STOREID').setReadOnly(true);
        f.findField('BEGINDATE').setReadOnly(true);
        f.findField('ENDDATE').setReadOnly(true);
        f.findField('SUMDATE').setReadOnly(true);
        f.findField('DATEBAS').setReadOnly(true);
        f.findField('MTHBAS').setReadOnly(true);
        f.findField('LASTDELI_MTH').setReadOnly(true);
        f.findField('LASTDELI_DT').setReadOnly(true);
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        T1Grid.getSelectionModel().deselectAll();
        Ext.getCmp('btnUpdate').setDisabled(true);
        Ext.getCmp('btnDel').setDisabled(true);
    }

    // 依庫備或非庫備決定可維護的欄位
    function setFormCol(fieldVal) {
        var f = T1Form.getForm();
        if (fieldVal == '0') {
            f.findField('BEGINDATE').setVisible(true);
            f.findField('ENDDATE').setVisible(true);
            f.findField('SUMDATE').setVisible(true);
            f.findField('MTHBAS').setVisible(true);
            f.findField('DATEBAS').setVisible(false);
            f.findField('BEGINDATE').allowBlank = false;
            f.findField('ENDDATE').allowBlank = false;
            f.findField('SUMDATE').allowBlank = false;
            f.findField('MTHBAS').allowBlank = false;
            f.findField('DATEBAS').allowBlank = true;
            f.findField('DATEBAS').setValue('');
        }
        else if (fieldVal == '1') {
            f.findField('BEGINDATE').setVisible(false);
            f.findField('ENDDATE').setVisible(false);
            f.findField('SUMDATE').setVisible(false);
            f.findField('MTHBAS').setVisible(false);
            f.findField('DATEBAS').setVisible(true);
            f.findField('BEGINDATE').allowBlank = true;
            f.findField('ENDDATE').allowBlank = true;
            f.findField('SUMDATE').allowBlank = true;
            f.findField('MTHBAS').allowBlank = true;
            f.findField('DATEBAS').allowBlank = false;
            f.findField('BEGINDATE').setValue('');
            f.findField('ENDDATE').setValue('');
            f.findField('SUMDATE').setValue('');
            f.findField('MTHBAS').setValue('');
        }
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '100%',
                        split: true,
                        items: [T1Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }
        ]
    });

    T1Load();
});