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
    var T1Name = "庫房基本檔維護";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_cancel_id = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0039/GetYN',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_wh_grade = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0039/GetWhGrade',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_wh_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0039/GetWhKind',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_whno_combo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0039/GetWhnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_pwhno_combo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0039/GetPWhnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Form.getForm().findField('WH_GRADE').getValue(),
                    p1: T1Form.getForm().findField('WH_NO').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        autoLoad: true
    });
    var st_inid_combo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0039/GetInidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
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
                fieldLabel: '庫房代碼',
                name: 'P0',
                id: 'P0',
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 8, // 可輸入最大長度為8
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
    });

    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
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
                xtype: 'combo',
                fieldLabel: '庫房代碼',
                name: 'P0',
                id: 'P0',
                store: st_whno_combo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
            },
            //{
            //    xtype: 'textfield',
            //    fieldLabel: '庫房代碼',
            //    name: 'P0',
            //    id: 'P0',
            //    enforceMaxLength: true, // 限制可輸入最大長度
            //    maxLength: 8, // 可輸入最大長度為8
            //    padding: '0 4 0 4'
            //},
            {
                xtype: 'combo',
                fieldLabel: '庫別分類',
                name: 'P1',
                id: 'P1',
                store: st_wh_kind,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '庫別級別',
                name: 'P2',
                id: 'P2',
                store: st_wh_grade,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '上級庫',
                name: 'P3',
                id: 'P3',
                store: st_whno_combo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
            }, {
                xtype: 'combo',
                fieldLabel: '責任中心',
                name: 'P4',
                id: 'P4',
                store: st_inid_combo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
            }, {
                xtype: 'combo',
                fieldLabel: '撥補責任中心',
                name: 'P5',
                id: 'P5',
                store: st_inid_combo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
            },{
                xtype: 'combo',
                fieldLabel: '是否作廢',
                name: 'P6',
                id: 'P6',
                store: st_cancel_id,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE'
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                T1Load();
                msglabel('訊息區:');
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
            }
        }]
    });
    var T1Store = Ext.create('WEBAPP.store.MI_WHMAST', { // 定義於/Scripts/app/store/MI_WHMAST.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1QueryForm.getForm().findField('P1').getValue(),
                    p2: T1QueryForm.getForm().findField('P2').getValue(),
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    p4: T1QueryForm.getForm().findField('P4').getValue(),
                    p5: T1QueryForm.getForm().findField('P5').getValue(),
                    p6: T1QueryForm.getForm().findField('P6').getValue()
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
                    T1Set = '/api/AA0039/Create'; // AA0039Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                    TATabs.setActiveTab('Form');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0039/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                    TATabs.setActiveTab('Form');
                }
            }, {
                xtype: 'button',
                id: 'btnExport',
                itemId: 'export', text: '匯出', handler: function () {

                    var p = new Array();
                    p.push({ name: 'FN', value: '庫房基本檔維護.xlsx' });
                    p.push({ name: 'p0', value: T1QueryForm.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1QueryForm.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1QueryForm.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1QueryForm.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1QueryForm.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T1QueryForm.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T1QueryForm.getForm().findField('P6').getValue() });
                    PostForm('/api/AA0039/Excel', p);
                    msglabel('訊息區:匯出完成');
                }
            }
            //, {
            //    itemId: 'delete', text: '刪除', disabled: true,
            //    handler: function () {
            //        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
            //            if (btn === 'yes') {
            //                T1Set = '/api/AA0039/Delete';
            //                T1Form.getForm().findField('x').setValue('D');
            //                T1Submit();
            //            }
            //        }
            //        );
            //    }
            //}
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.MI_WHMAST'); // /Scripts/app/model/MI_WHMAST.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("WH_NO"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('CANCEL_ID').setValue('N'); // 作廢預設為N
        }
        else {
            u = f.findField('WH_NO');
        }
        f.findField('x').setValue(x);
        f.findField('WH_NAME').setReadOnly(false);
        f.findField('WH_KIND').setReadOnly(false);
        f.findField('WH_GRADE').setReadOnly(false);
        f.findField('PWH_NO').setReadOnly(false);
        f.findField('INID').setReadOnly(false);
        f.findField('SUPPLY_INID').setReadOnly(false);
        f.findField('TEL_NO').setReadOnly(false);
        f.findField('CANCEL_ID').setReadOnly(false);
        f.findField('SD1').setReadOnly(false);
        f.findField('SD2').setReadOnly(false);
        f.findField('SD3').setReadOnly(false);
        f.findField('SD4').setReadOnly(false);
        f.findField('SD5').setReadOnly(false);
        f.findField('SD6').setReadOnly(false);
        f.findField('SD7').setReadOnly(false);
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
        //    dock: 'top',
        //    xtype: 'toolbar',
        //    layout: 'fit',
        //    items: [T1Query]
        //}, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        },{
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 80
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 130
        }, {
            text: "庫別分類",
            dataIndex: 'WH_KIND_N',
            width: 80
        }, {
            text: "庫別級別",
            dataIndex: 'WH_GRADE_N',
            width: 80
        }, {
            text: "上級庫",
            dataIndex: 'PWH_NO_N',
            width: 150
        }, {
            text: "責任中心",
            dataIndex: 'INID_N',
            width: 150
        }, {
            text: "撥補責任中心",
            dataIndex: 'SUPPLY_INID_N',
            width: 150
        }, {
            text: "電話分機",
            dataIndex: 'TEL_NO',
            width: 80
        }, {
            text: "是否作廢",
            dataIndex: 'CANCEL_ID_N',
            width: 80
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
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('WH_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('WH_KIND').setReadOnly(true);
            f.findField('WH_GRADE').setReadOnly(true);
            f.findField('PWH_NO').setReadOnly(true);
            f.findField('INID').setReadOnly(true);
            f.findField('SUPPLY_INID').setReadOnly(true);
            f.findField('CANCEL_ID').setReadOnly(true);
            if (T1LastRec.data['CANCEL_ID'] == 'Y') {
                T1Grid.down('#edit').setDisabled(true); // 作廢的資料就不允許修改
            }

            Ext.getCmp('SD1').setValue(false);
            Ext.getCmp('SD2').setValue(false);
            Ext.getCmp('SD3').setValue(false);
            Ext.getCmp('SD4').setValue(false);
            Ext.getCmp('SD5').setValue(false);
            Ext.getCmp('SD6').setValue(false);
            Ext.getCmp('SD7').setValue(false);
            if (T1LastRec.data['D1'] == 'Y') {
                Ext.getCmp('SD1').setValue(true);
            }
            if (T1LastRec.data['D2'] == 'Y') {
                Ext.getCmp('SD2').setValue(true);
            }
            if (T1LastRec.data['D3'] == 'Y') {
                Ext.getCmp('SD3').setValue(true);
            }
            if (T1LastRec.data['D4'] == 'Y') {
                Ext.getCmp('SD4').setValue(true);
            }
            if (T1LastRec.data['D5'] == 'Y') {
                Ext.getCmp('SD5').setValue(true);
            }
            if (T1LastRec.data['D6'] == 'Y') {
                Ext.getCmp('SD6').setValue(true);
            }
            if (T1LastRec.data['D7'] == 'Y') {
                Ext.getCmp('SD7').setValue(true);
            }
            f.findField('SD1').setReadOnly(true);
            f.findField('SD2').setReadOnly(true);
            f.findField('SD3').setReadOnly(true);
            f.findField('SD4').setReadOnly(true);
            f.findField('SD5').setReadOnly(true);
            f.findField('SD6').setReadOnly(true);
            f.findField('SD7').setReadOnly(true);
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
        }, {
            name: 'D1',
            xtype: 'hidden'
        }, {
            name: 'D2',
            xtype: 'hidden'
        }, {
            name: 'D3',
            xtype: 'hidden'
        }, {
            name: 'D4',
            xtype: 'hidden'
        }, {
            name: 'D5',
            xtype: 'hidden'
        }, {
            name: 'D6',
            xtype: 'hidden'
        }, {
            name: 'D7',
            xtype: 'hidden'
        },{
            fieldLabel: '庫房代碼',
            name: 'WH_NO',
            enforceMaxLength: true,
            maxLength: 8,
            regexText: '只能輸入英文字母與數字',
            regex: /^[\w]{0,10}$/, // 用正規表示式限制可輸入內容
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required'
            ,
            listeners: {
                select: function (combo, record, index) {
                    st_pwhno_combo.load();
                }
            }
        }, {
            fieldLabel: '庫房名稱',
            name: 'WH_NAME',
            enforceMaxLength: true,
            maxLength: 40,
            readOnly: true
        }, {
            xtype: 'combo',
            fieldLabel: '庫別分類',
            name: 'WH_KIND',
            store: st_wh_kind,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            editable: false,
            forceSelection: true,
            readOnly: true,
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
        }, {
            xtype: 'combo',
            fieldLabel: '庫別級別',
            name: 'WH_GRADE',
            store: st_wh_grade,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            editable: false,
            forceSelection: true,
            readOnly: true,
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            listeners: {
                select: function (combo, record, index) {
                    st_pwhno_combo.load();
                }
            }
        },  {
            xtype: 'combo',
            fieldLabel: '上級庫',
            name: 'PWH_NO',
            store: st_pwhno_combo,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            editable: false,
            forceSelection: true,
            readOnly: true,
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
        }, {
            xtype: 'combo',
            fieldLabel: '責任中心',
            name: 'INID',
            store: st_inid_combo,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            anyMatch: true,
            forceSelection: true,
            typeAhead: true,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'combo',
            fieldLabel: '撥補責任中心',
            name: 'SUPPLY_INID',
            store: st_inid_combo,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            anyMatch: true,
            forceSelection: true,
            typeAhead: true,
            readOnly: true
        }, {
            fieldLabel: '電話分機',
            name: 'TEL_NO',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true
        }, {
            xtype: 'combo',
            fieldLabel: '是否作廢',
            name: 'CANCEL_ID',
            store: st_cancel_id,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            editable: false,
            forceSelection: true,
            readOnly: true
        },
        {
            xtype: 'container',
            id: 'SDTIME', 
            layout: {
                type: 'vbox',
                align: 'center'
            }, 
            items: [
                {
                    xtype: 'label',
                    text: '自動撥補時間',
                    style: 'color: #ff0000;'
                }, {
                    xtype: 'checkboxfield', fieldLabel: '星期日', labelAlign: 'right', name: 'SD1', id: 'SD1',
                    readOnly: true, width: 100, labelWidth: 80,
                    listeners: {
                        change: function (checkbox, newValue, oldValue, eOpts) {
                            if (checkbox.checked) {
                                T1Form.getForm().findField('D1').setValue("Y");
                            } else {
                                T1Form.getForm().findField('D1').setValue("N");
                            }
                        }
                    }
                }, {
                    xtype: 'checkboxfield', fieldLabel: '星期一', labelAlign: 'right', name: 'SD2', id: 'SD2',
                    readOnly: true, width: 100, labelWidth: 80,
                    listeners: {
                        change: function (checkbox, newValue, oldValue, eOpts) {
                            if (checkbox.checked) {
                                T1Form.getForm().findField('D2').setValue("Y");
                            } else {
                                T1Form.getForm().findField('D2').setValue("N");
                            }
                        }
                    }
                }, {
                    xtype: 'checkboxfield', fieldLabel: '星期二', labelAlign: 'right', name: 'SD3', id: 'SD3',
                    readOnly: true, width: 100, labelWidth: 80,
                    listeners: {
                        change: function (checkbox, newValue, oldValue, eOpts) {
                            if (checkbox.checked) {
                                T1Form.getForm().findField('D3').setValue("Y");
                            } else {
                                T1Form.getForm().findField('D3').setValue("N");
                            }
                        }
                    }
                }, {
                    xtype: 'checkboxfield', fieldLabel: '星期三', labelAlign: 'right', name: 'SD4', id: 'SD4',
                    readOnly: true, width: 100, labelWidth: 80,
                    listeners: {
                        change: function (checkbox, newValue, oldValue, eOpts) {
                            if (checkbox.checked) {
                                T1Form.getForm().findField('D4').setValue("Y");
                            } else {
                                T1Form.getForm().findField('D4').setValue("N");
                            }
                        }
                    }
                }, {
                    xtype: 'checkboxfield', fieldLabel: '星期四', labelAlign: 'right', name: 'SD5', id: 'SD5',
                    readOnly: true, width: 100, labelWidth: 80,
                    listeners: {
                        change: function (checkbox, newValue, oldValue, eOpts) {
                            if (checkbox.checked) {
                                T1Form.getForm().findField('D5').setValue("Y");
                            } else {
                                T1Form.getForm().findField('D5').setValue("N");
                            }
                        }
                    }
                }, {
                    xtype: 'checkboxfield', fieldLabel: '星期五', labelAlign: 'right', name: 'SD6', id: 'SD6',
                    readOnly: true, width: 100, labelWidth: 80,
                    listeners: {
                        change: function (checkbox, newValue, oldValue, eOpts) {
                            if (checkbox.checked) {
                                T1Form.getForm().findField('D6').setValue("Y");
                            } else {
                                T1Form.getForm().findField('D6').setValue("N");
                            }
                        }
                    }
                }, {
                    xtype: 'checkboxfield', fieldLabel: '星期六', labelAlign: 'right', name: 'SD7', id: 'SD7',
                    readOnly: true, width: 100, labelWidth: 80,
                    listeners: {
                        change: function (checkbox, newValue, oldValue, eOpts) {
                            if (checkbox.checked) {
                                T1Form.getForm().findField('D7').setValue("Y");
                            } else {
                                T1Form.getForm().findField('D7').setValue("N");
                            }
                        }
                    }
                }
            ]
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if (this.up('form').getForm().findField('WH_NO').getValue() == ''
                    ) //&& this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                        Ext.Msg.alert('提醒', '至少需輸入');
                    else {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        }
                        );
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
                            msglabel('訊息區:資料修改成功');
                            break;
                        //case "D":
                        //    T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                        //    r.commit();
                        //    break;
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

    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1QueryForm]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form]
        }]
    });
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
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });

    function getHospCode() {
        Ext.Ajax.request({
            url: '/api/FA0067/GetHospCode', //與FA0067共用取得醫院代碼
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg != '0') {
                        Ext.getCmp('SDTIME').hide();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getHospCode();

    T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
});
