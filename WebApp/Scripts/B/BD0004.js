Ext.define('overrides.selection.CheckboxModel', {
    override: 'Ext.selection.CheckboxModel',

    getHeaderConfig: function () {
        var config = this.callParent();
        if (Ext.isFunction(this.selectable)) {

            config.selectable = this.selectable;
            config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                if (this.selectable(record)) {
                    record.selectable = false;
                    return '';
                }
                record.selectable = true;
                return this.defaultRenderer();
            };
            this.on('beforeselect', function (rowModel, record, index, eOpts) {
                return !this.selectable(record);
            }, this);
        }

        return config;
    },

    updateHeaderState: function () {
        // check to see if all records are selected
        var me = this,
            store = me.store,
            storeCount = store.getCount(),
            views = me.views,
            hdSelectStatus = false,
            selectedCount = 0,
            selected, len, i, notSelectableRowsCount = 0;

        if (!store.isBufferedStore && storeCount > 0) {
            hdSelectStatus = true;
            store.each(function (record) {
                if (!record.selectable) {
                    notSelectableRowsCount++;
                }
            }, this);
            selected = me.selected;

            for (i = 0, len = selected.getCount(); i < len; ++i) {
                if (store.indexOfId(selected.getAt(i).id) > -1) {
                    ++selectedCount;
                }
            }
            hdSelectStatus = storeCount === selectedCount + notSelectableRowsCount;
        }

        if (views && views.length) {
            me.column.setHeaderStatus(hdSelectStatus);
        }
    }
});

Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

//Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';

    var T1Name = "訂單EMAIL發送";

    var T1Rec = 0;
    var T1LastRec = null;
    var docno = '';
    var ps = '';
    var MATComboGet = '../../../api/BA0002/GetMATCombo';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    // 物品類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('P4').setValue(wh_nos[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setComboData();
    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
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
                    xtype: 'datefield',
                    id: 'P1',
                    name: 'P1',
                    fieldLabel: '申請日期',
                    labelWidth: 60,
                    width: 150,
                    padding: '0 4 0 4',
                    allowBlank: false,
                    fieldCls: 'required',
                    value: getDefaultValue(false),
                    regexText: '請選擇日期',
                }, {
                    xtype: 'datefield',
                    id: 'P2',
                    name: 'P2',
                    fieldLabel: '至',
                    width: 88,
                    labelWidth: 8,
                    padding: '0 4 0 4',
                    allowBlank: false,
                    fieldCls: 'required',
                    value: getDefaultValue(true),
                    regexText: '請選擇日期',
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P4',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    padding: '0 4 0 4',
                    store: MATQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'radiogroup',
                    anchor: '40%',
                    labelWidth: 60,
                    fieldLabel: '合約種類',
                    width: 240,
                    items: [
                        { boxLabel: '合約', width: 100, name: 'P3', inputValue: '0', checked: true },
                        { boxLabel: '非合約', width: 100, name: 'P3', inputValue: '2' }
                    ]

                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        //getINID(parent.parent.userId);
                        docno = "";
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P1').focus();
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    function getDefaultValue(isEndDate) {
        var yyyy = new Date().getFullYear() - 1911;
        var m = new Date().getMonth() + 1;
        var d = 0;
        if (isEndDate) {
            d = new Date(yyyy, m, 0).getDate();
        } else {
            d = 1;
        }
        var mm = m > 9 ? m.toString() : "0" + m.toString();
        var dd = d > 9 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;

    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [' PO_NO', 'AGEN_NO', 'MSGTEXT ', 'PO_STATUS', 'MEMO', 'SMEMO', 'CREATE_TIME', 'CREATE_USER', 'UPDATE_TIME', 'UPDATE_USER', 'UPDATE_IP', 'EMAIL']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 1000,
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().getValues()['P3'],
                    p4: T1Query.getForm().findField('P4').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0004/MasterAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }


    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_C', 'MMNAME_E', 'M_AGENLAB', 'M_PURUN', 'PO_PRICE', 'PO_QTY', 'PO_AMT', 'MEMO']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });
    var T11Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: docno
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0004/DetailAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });
    function T11Load() {
        T11Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        //displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BD0004/MasterUpdate';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1delete', text: '寄送MAIL', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    var records = T1Grid.getSelectionModel().getSelection();
                    //console.log(records[0].data.PO_NO);
                    // return;
                    Ext.MessageBox.confirm('寄送MAIL', '是否確定寄送MAIL？', function (btn, text) {
                        if (btn === 'yes') {
                            for (var i = 0; i < records.length; i++) {

                                Ext.Ajax.request({
                                    url: '/api/BD0004/MasterUpdateMAIL',
                                    actionMethods: {
                                        read: 'POST' // by default GET
                                    },
                                    params: {
                                        pono: records[i].data.PO_NO,

                                    },
                                    success: function (response) {


                                    },
                                    failure: function (response, options) {

                                    }
                                });
                            }

                            T1Load();
                            msglabel('訊息區:資料修改成功');
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        //alert(parent.parent.userId);
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            f.clearInvalid();

        }

        f.findField('x').setValue(x);
        f.findField('MEMO').setReadOnly(false);
        f.findField('SMEMO').setReadOnly(false);
        T1Form.down('#t1cancel').setVisible(true);
        T1Form.down('#t1submit').setVisible(true);
        //u.focus();

    }


    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        displayInfo: true,
        border: false,
        plain: true,
    });


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
            text: "訂單編號",
            dataIndex: 'PO_NO',
            style: 'text-align:left',
            align: 'left',
            width: 140
        }, {
            text: "廠商",
            dataIndex: 'AGEN_NO',
            style: 'text-align:left',
            align: 'left',
            width: 110
        }, {
            text: "訂單狀態",
            dataIndex: 'PO_STATUS',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "MAIL備註",
            dataIndex: 'MEMO',
            style: 'text-align:left',
            align: 'left',
            width: 250
        }, {
            text: "MAIL特殊備註",
            dataIndex: 'SMEMO',
            style: 'text-align:left',
            align: 'left',
            width: 200
        }, {
            text: "廠商MAIL",
            dataIndex: 'EMAIL',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "緊急醫療出貨",
            dataIndex: 'ISCR',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            header: "",
            flex: 1
        }],


        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox

            selectable: function (record) {
                console.log(record)
                //return record.get('PO_STATUS') !=='開單';
            },

            //renderer: function (v, p, record) {//extjs selModel 不支援rendrer
            //    alert(record.data.PO_STATUS);
            //    console.log(record);
            //    if (record.data.PO_STATUS !=='開單')

            //        return ''; // 不顯示勾選框
            //    else
            //        return '<div class="x-grid-row-checker">&nbsp;</div>';
            //}
        },
        ),


        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        // T11Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                var recos = T1Grid.getSelectionModel().getSelection();
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                //console.log(recos.length)
                if (recos.length > 0) {
                    docno = recos[0].data.PO_NO;
                    ps = recos[0].data.PO_STATUS;

                }

                //ComboData();
                //if (recos[0].data.PO_STATUS == '已開單') {
                setFormT1a();
                //}
                T11Store.removeAll();

                T11Load();
            }
        },


    });
    function setFormT1a() {
        //alert("1")
        T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
        T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            //if (T1LastRec.data['STATUS'].split(' ')[0] != 'A' && T1LastRec.data['STATUS'].split(' ')[0] != 'D') {
            // 非新增/剔退的資料就不允許修改/刪除
            //alert(ps)
            //alert("2")
            if (ps == '開單') {
                T1Grid.down('#t1edit').setDisabled(false);
                T1Grid.down('#t1delete').setDisabled(false);
            }
            else {
                T1Grid.down('#t1edit').setDisabled(true);
                T1Grid.down('#t1delete').setDisabled(true);

            }
            //T1Grid.down('#t1audit').setDisabled(true);
            //T11Grid.down('#t11add').setDisabled(false);
            // }
            //else {
            //    //T1Grid.down('#t1audit').setDisabled(false);
            //    T11Grid.down('#t11add').setDisabled(false);
            //}
            //T1Grid.down('#t1print').setDisabled(false);

        }
        else {
            T1Form.getForm().reset();
            //T1Grid.down('#t1audit').setDisabled(true);
            //T1Grid.down('#t1print').setDisabled(true);
            // T11Grid.down('#t11add').setDisabled(true);
            //T11Grid.down('#t11edit').setDisabled(true);
            // T11Grid.down('#t11delete').setDisabled(true);
        }
    }

    var T11Grid = Ext.create('Ext.grid.Panel', {
        store: T11Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T11Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            align: 'left',
            width: 100
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            align: 'left',
            width: 100
        }, {
            text: "廠牌",
            dataIndex: 'M_AGENLAB',
            align: 'left',
            width: 100
        }, {
            text: "單位",
            dataIndex: 'M_PURUN',
            align: 'left',
            width: 100
        }, {
            text: "單價",
            dataIndex: 'PO_PRICE',
            align: 'right',
            width: 100
        }, {
            text: "申購量",
            dataIndex: 'PO_QTY',
            align: 'right',
            width: 100
        }, {
            text: "單筆價",
            dataIndex: 'PO_AMT',
            align: 'right',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            align: 'left',
            width: 100
        }, {
            header: "",
            flex: 1
        }],

    });


    var T1Form = Ext.widget({
        xtype: 'form',
        layout: { type: 'table', columns: 1 },
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
            fieldLabel: '訂單編號',
            name: 'PO_NO',
            readOnly: true
        }, {
            fieldLabel: '廠商',
            name: 'AGEN_NO',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            fieldLabel: '訂單狀態',
            name: 'PO_STATUS',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '訂單狀態',
            name: 'EMAIL',
            enforceMaxLength: true,
            readOnly: true,
            submitValue: true
        }, {
            xtype: 'textareafield',
            fieldLabel: 'MAIL備註',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 4000,
            readOnly: true,
            height: 200,
            width: "100%"
        }, {
            xtype: 'textareafield',
            fieldLabel: 'MAIL特殊備註',
            name: 'SMEMO',
            enforceMaxLength: true,
            maxLength: 4000,
            readOnly: true,
            height: 200,
            width: "100%"
        }],
        buttons: [{
            itemId: 't1submit', text: '儲存', hidden: true,
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
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't1cancel', text: '取消', hidden: true, handler: T1Cleanup
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
                            //T1Cleanup();
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            // T1Query.getForm().findField('P0').setValue(v.MGROUP);
                            T1Load();
                            //T1Cleanup();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料資料修改成功');
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();

                            msglabel('訊息區:送審完成');
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
        viewport.down('#t11Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if ((fc.xtype === "displayfield") || (fc.xtype === "textfield") || (fc.xtype === "combo")) {
                fc.setReadOnly(true);
            } else if (fc.xtype === "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#t1cancel').hide();
        T1Form.down('#t1submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
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
        items: [
            {
                itemId: 'tGrid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [{
                    //  xtype:'container',
                    region: 'center',
                    layout: {
                        type: 'border',
                        padding: 0
                    },
                    collapsible: false,
                    title: '',
                    split: true,
                    width: '80%',
                    flex: 1,
                    minWidth: 50,
                    minHeight: 140,
                    items: [
                        {
                            itemId: 't1Grid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
                            items: [T1Grid]
                        }, {
                            itemId: 't11Grid',
                            region: 'south',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            height: '50%',
                            split: true,
                            items: [T11Grid]
                        }
                    ]
                }]
            },
            {
                itemId: 'form',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
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
    T1Query.getForm().findField('P1').focus();
    T1Load();
});