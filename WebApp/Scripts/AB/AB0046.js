Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var viewModel = Ext.create('WEBAPP.store.AB.AB0046VM');   // 定義於/Scripts/app/store/AB/AB0046VM.js  
    /////var WHStore = viewModel.getStore('CD0007WH_NO');
    var T1Store = viewModel.getStore('ME_AB0046');            // 定義於/Scripts/app/store/AB0046VM.js

    var T1Set = ''; // 新增/修改/刪除
    var T1Get = '../../../api/AB0046/All';                   // ../../../api/AB0046/All
 
    var VisitKindComboGet = '../../../api/AB0046/VisitKindCombo';
    var LocationComboGet = '../../../api/AB0046/LocationCombo';

    var CommonComboGet = '../../../api/AB0046/CommonCombo';

    var T1Name = "預設扣庫單位動向維護檔";
    var T1Name2 = "安全量、作業量";

    var T1Rec = 0;
    
    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 門住別 下拉 combo查詢
    var VisitKindStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
                { VALUE : '0', TEXT : '不分'},
                { VALUE : '1', TEXT: '住院' },
                { VALUE : '2', TEXT: '門診' },
                { VALUE : '3', TEXT: '急診' }
        ],
        autoLoad: true
    });

    var LocationStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: LocationComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var CommonStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: CommonComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 85;

    //////var T1Query = Ext.widget({
    //////    xtype: 'form',
    //////    layout: 'form',
    //////    border: false,
    //////    autoScroll: true,     // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
    //////    fieldDefaults: {
    //////        xtype: 'textfield',
    //////        labelAlign: 'right',
    //////        labelWidth: mLabelWidth,
    //////        width: mWidth
    //////    },
    //////    items: [
    //////        {
    //////            xtype: 'panel',
    //////            id: 'PanelP1',
    //////            border: false,
    //////            layout: 'hbox',
    //////            defaults: {
    //////                anchor: '100%',
    //////                editable: true,
    //////                forceSelection: true,
    //////                labelAlign: 'right',
    //////            },
    //////            items: [
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreFr,
    //////                    name: 'RoutinStockCode',
    //////                    id: 'RoutinStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: '例行扣庫',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '0 26 0 0'
    //////                },  {
    //////                    xtype: 'datefield',
    //////                    id: 'BeginTime',
    //////                    name: 'BeginTime',
    //////                    fieldLabel: '例行時間起',
    //////                    width: 185,
    //////                    labelWidth: 90,
    //////                    vtype: 'dateRange',
    //////                    dateRange: { end: 'EndTime' },
    //////                    padding: '0 2 0 0'
    //////                },  {
    //////                    xtype: 'timefield',
    //////                    name: 'D4',
    //////                    id: 'D4',
    //////                    reference: 'timeField',
    //////                    format: 'H:i',
    //////                    maxValue: '24:00',
    //////                    increment: 5,
    //////                    width: 92,
    //////                    padding: '0 12 0 0'
    //////                },  {
    //////                    xtype: 'datefield',
    //////                    id: 'EndTime',
    //////                    name: 'EndTime',
    //////                    fieldLabel: '例行時間迄',
    //////                    width: 175,
    //////                    labelWidth: 82,
    //////                    vtype: 'dateRange',
    //////                    dateRange: { begin: 'BeginTime' },
    //////                    padding: '0 2 0 0'
    //////                },  {
    //////                    xtype: 'timefield',
    //////                    name: 'D5',
    //////                    id: 'D5',
    //////                    reference: 'timeField',
    //////                    format: 'H:i',
    //////                    maxValue: '24:00',
    //////                    increment: 5,
    //////                    width: 95,
    //////                    padding: '0 50 0 0'
    //////                },
    //////                {
    //////                    xtype: 'button',
    //////                    text: '查詢',
    //////                    handler: function () {
    //////                        //if ((T1Query.getForm().findField('QryDateFr').getValue() == "" || T1Query.getForm().findField('QryDateFr').getValue() == null) ||
    //////                        //    (T1Query.getForm().findField('QryDateTo').getValue() == "" || T1Query.getForm().findField('QryDateTo').getValue() == null)) {

    //////                        //    Ext.Msg.alert('訊息', '需填查詢月份才能查詢');
    //////                        //}
    //////                        //else {
    //////                            T1Load();
    //////                            msglabel('訊息區:');
    //////                        //}
    //////                    }
    //////                },
    //////                {
    //////                    xtype: 'button',
    //////                    text: '清除',
    //////                    style: 'margin:0px 5px;',
    //////                    handler: function () {
    //////                        var f = this.up('form').getForm();
    //////                        f.reset();
    //////                        f.findField('RoutinStockCode').focus(); // 進入畫面時輸入游標預設在P0
    //////                        msglabel('訊息區:');
    //////                    }
    //////                }
    //////            ]
    //////        }
    //////        ,
    //////        {
    //////            xtype: 'panel',
    //////            id: 'PanelP2',
    //////            border: false,
    //////            layout: 'hbox',
    //////            defaults: {
    //////                anchor: '100%',
    //////                editable: true,
    //////                forceSelection: true,
    //////                labelAlign: 'right',
    //////            },
    //////            items: [
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreFr,
    //////                    name: 'exceptStockCode',
    //////                    id: 'exceptStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: '非例行扣庫',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '2 26 0 0'
    //////                },
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreTo,
    //////                    name: 'takeOutStockCode',
    //////                    id: 'takeOutStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: '出院帶藥扣庫',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '2 4 0 0'
    //////                }
    //////                ,
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreTo,
    //////                    name: 'returnStockCode',
    //////                    id: 'returnStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: '退藥庫別',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '2 4 0 0'
    //////                }
    //////            ]
    //////        }
    //////        ,
    //////        {
    //////            xtype: 'panel',
    //////            id: 'PanelP3',
    //////            border: false,
    //////            layout: 'hbox',
    //////            defaults: {
    //////                anchor: '100%',
    //////                editable: true,
    //////                forceSelection: true,
    //////                labelAlign: 'right',
    //////            },
    //////            items: [
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreFr,
    //////                    name: 'chemoStockCode',
    //////                    id: 'chemoStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: '化療扣庫',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '2 26 0 0'
    //////                },
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreTo,
    //////                    name: 'tpnStockCode',
    //////                    id: 'tpnStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: 'TPN扣庫',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '2 4 0 0'
    //////                }
    //////                ,
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreTo,
    //////                    name: 'pcaStockCode',
    //////                    id: 'pcaStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: 'PCA扣庫',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '2 4 0 0'
    //////                }
    //////            ]
    //////        }
    //////        ,
    //////        {
    //////            xtype: 'panel',
    //////            id: 'PanelP4',
    //////            border: false,
    //////            layout: 'hbox',
    //////            defaults: {
    //////                anchor: '100%',
    //////                editable: false,
    //////                forceSelection: true,
    //////                labelAlign: 'right',
    //////            },
    //////            items: [
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreFr,
    //////                    name: 'priorityStockCode',
    //////                    id: 'priorityStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: '優先扣庫',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '2 26 0 0'
    //////                },
    //////                {
    //////                    xtype: 'combobox',
    //////                    store: OrderDrStoreTo,
    //////                    name: 'researchStockCode',
    //////                    id: 'researchStockCode',
    //////                    width: 280,
    //////                    labelWidth: 90,
    //////                    fieldLabel: '研究用藥扣庫',
    //////                    displayField: 'CHINNAME',
    //////                    valueField: 'ORDERDR',
    //////                    queryMode: 'local',
    //////                    padding: '2 4 0 0'
    //////                }
    //////            ]
    //////        }
    //////    ]

    //////});

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,     // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP5',
                border: false,
                layout: 'hbox',
                defaults: {
                    anchor: '100%',
                    editable: false,
                    forceSelection: true,
                    labelAlign: 'right',
                },
                items: [
                    {
                        xtype: 'combobox',
                        store: VisitKindStore,
                        id: 'VisitKind',
                        name: 'VisitKind',
                        fieldLabel: '門住別',
                        width: 280,
                        labelWidth: 60,
                        queryMode: 'local',
                        editable: true,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        padding: '0 4 0 0'
                    },
                    {
                        xtype: 'combobox',
                        store: LocationStore,
                        width: 280,
                        labelWidth: 90,
                        id: 'Location',
                        name: 'Location',
                        //labelSeparator: '',
                        editable: true,
                        fieldLabel: '診間/病房代碼',
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        padding: '0 4 0 0'
                    }
                    ,
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: T1Load
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('VISITKIND').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
                    }
                ]
            }
        ]

    });


    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('VisitKind').getValue());           // 門住別
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('Location').getValue());            // 診間/病房代碼
        T1Store.load({
            params: {
                start: 0
            }
            ,
            callback: function (records, operation, success) {
                if (success) {
                    T1RecLength = records.length;
                    if (records.length == 0) {
                    } else {
                        T1Grid.down('#edit').setDisabled(T1RecLength === 0);        //新增完後T1RecLength是0，無法修改
                        T1Grid.down('#delete').setDisabled(T1RecLength === 0);      //選擇左側T1Grid，一個會有T1LastRec，則可以修改
                    }
                } else {
                    Ext.Msg.alert('訊息', 'failure!');
                }
            }
        })

        //////////T1Tool.moveFirst();
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
                    T1Set = '/api/AB0046/Create'; // AA0048Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0046/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0046/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }
        ]
    });

    function setFormT1(x, t) {          //做畫面隱藏等控制
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name2);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            T1Form.getForm().reset();
            var r = Ext.create('WEBAPP.model.ME_AB0046'); // /Scripts/app/model/ME_AB0046.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容;可問
            // u = f.findField("VISITKIND"); //在新增時才可填寫
            // u.setReadOnly(false);
            f.findField('VISITKIND').setReadOnly(false);
            f.findField('LOCATION').setReadOnly(false);
            f.findField('ROUTINESTOCKCODE').setReadOnly(false);
            f.findField('BEGINTIME').setReadOnly(false);
            f.findField('ENDTIME').setReadOnly(false);
            f.findField('EXCEPTSTOCKCODE').setReadOnly(false);
            f.findField('TAKEOUTSTOCKCODE').setReadOnly(false);
            f.findField('RETURNSTOCKCODE').setReadOnly(false);

            f.findField('CHEMOSTOCKCODE').setReadOnly(false);
            f.findField('TPNSTOCKCODE').setReadOnly(false);
            f.findField('PCASTOCKCODE').setReadOnly(false);
            f.findField('DEFAULTSTOCKCODE').setReadOnly(false);
            f.findField('RESEARCHSTOCKCODE').setReadOnly(false);
        }
        else {
               u = f.findField('VISIT_KIND');
               f.findField('VISITKIND').setReadOnly(true);
               f.findField('LOCATION').setReadOnly(true);
               f.findField('ROUTINESTOCKCODE').setReadOnly(false);
               f.findField('BEGINTIME').setReadOnly(false);
               f.findField('ENDTIME').setReadOnly(false);
               f.findField('EXCEPTSTOCKCODE').setReadOnly(false);
               f.findField('TAKEOUTSTOCKCODE').setReadOnly(false);
               f.findField('RETURNSTOCKCODE').setReadOnly(false);

               f.findField('CHEMOSTOCKCODE').setReadOnly(false);
               f.findField('TPNSTOCKCODE').setReadOnly(false);
               f.findField('PCASTOCKCODE').setReadOnly(false);
               f.findField('DEFAULTSTOCKCODE').setReadOnly(false);
               f.findField('RESEARCHSTOCKCODE').setReadOnly(false);
        }
        f.findField('x').setValue(x);  //T1Form設定為I or U or D
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
    }

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
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
            text: "門住別",
            dataIndex: 'VISITKIND',
            width: 90,
            renderer: function (value) {
                if (value == '0')
                    return "不分";
                else if (value == '1')
                    return "住院";
                else if (value == '2')
                    return "門診";
                else if (value == '3')
                    return "急診";
            }
        }, {
            text: "診間/護理站",
            dataIndex: 'LOCATION',
            width: 80
        }, {
            text: "例行扣庫",
            dataIndex: 'ROUTINESTOCKCODE',
            width: 88
        }, {
            text: "院內頻率",
            dataIndex: 'FREQNO',
            width: 80
        }, {
            text: "例行時間起",
            dataIndex: 'BEGINTIME',
            width: 80
        }, {
            text: "例行時間迄",
            dataIndex: 'ENDTIME',
            width: 80
        }, {
            text: "非例行扣庫",
            dataIndex: 'EXCEPTSTOCKCODE',
            width: 80
        }, {
            text: "出院帶藥",
            dataIndex: 'TAKEOUTSTOCKCODE',
            width: 80
        }, {
            text: "退藥庫別",
            dataIndex: 'RETURNSTOCKCODE',
            width: 80
        }, {
            text: "化療扣庫",
            dataIndex: 'CHEMOSTOCKCODE',
            width: 80
        }, {
            text: "TPN扣庫別",
            dataIndex: 'TPNSTOCKCODE',
            width: 80
        }, {
            text: "PCA扣庫別",
            dataIndex: 'PCASTOCKCODE',
            width: 80
        }, {
            text: "優先扣庫",
            dataIndex: 'DEFAULTSTOCKCODE',
            width: 80
        }, {
            text: "研究用藥扣庫",
            dataIndex: 'RESEARCHSTOCKCODE',
            width: 120
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

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
            { //控制項為了Update Insert Delete
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }, { 
                fieldLabel: '院內頻率',
                name: 'FREQNO',
                id : 'FREQNO',
                hidden: true
            }, {
                xtype: 'combo',
                store: VisitKindStore,
                name: 'VISITKIND',
                id: 'VISITKIND',
                fieldLabel: '門住別',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            },  {
                xtype: 'combo',
                store: LocationStore,
                name: 'LOCATION',
                id: 'LOCATION',
                fieldLabel: '診間/護理站',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            },  {
                xtype: 'combo',
                store: LocationStore,
                name: 'ROUTINESTOCKCODE',
                id: 'ROUTINESTOCKCODE',
                fieldLabel: '例行扣庫',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            }, {
                xtype: 'timefield',
                name: 'BEGINTIME',
                id: 'BEGINTIME',
                fieldLabel: '例行時間起',
                id: 'BEGINTIME',
                reference: 'timeField',
                format: 'H:i',
                maxValue: '24:00',
                increment: 5,
                width: 92,
                readOnly: true
            }, {
                xtype: 'timefield',
                fieldLabel: '例行時間迄',
                name: 'ENDTIME',
                id: 'ENDTIME',
                reference: 'timeField',
                format: 'H:i',
                maxValue: '24:00',
                increment: 5,
                width: 92,
                readOnly: true
            }, {
                xtype: 'combo',
                store: LocationStore,
                name: 'EXCEPTSTOCKCODE',
                id: 'EXCEPTSTOCKCODE',
                fieldLabel: '非例行扣庫',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            }, {
                xtype: 'combo',
                store: CommonStore,
                name: 'TAKEOUTSTOCKCODE',
                id: 'TAKEOUTSTOCKCODE',
                fieldLabel: '出院帶藥',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            }, {
                xtype: 'combo',
                store: CommonStore,
                name: 'RETURNSTOCKCODE',
                id: 'RETURNSTOCKCODE',
                fieldLabel: '退藥庫別',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            }, {
                xtype: 'combo',
                store: CommonStore,
                name: 'CHEMOSTOCKCODE',
                id: 'CHEMOSTOCKCODE',
                fieldLabel: '化療扣庫',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            }, {
                xtype: 'combo',
                store: CommonStore,
                name: 'TPNSTOCKCODE',
                id: 'TPNSTOCKCODE',
                fieldLabel: 'TPN扣庫別',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            }, {
                xtype: 'combo',
                store: CommonStore,
                name: 'PCASTOCKCODE',
                id: 'PCASTOCKCODE',
                fieldLabel: 'PCA扣庫別',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            }, {
                xtype: 'combo',
                store: CommonStore,
                name: 'DEFAULTSTOCKCODE',
                id: 'DEFAULTSTOCKCODE',
                fieldLabel: '優先扣庫',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
               
            }, {
                xtype: 'combo',
                store: CommonStore,
                name: 'RESEARCHSTOCKCODE',
                id: 'RESEARCHSTOCKCODE',
                fieldLabel: '研究用藥扣庫',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }

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
                    });

                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
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
                url: T1Set,  //導到後端API
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) { //insert update 問
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            //T1Query.getForm().findField('P0').setValue(f2.findField('WH_NO').getValue());
                            //T1Query.getForm().findField('P1').setValue(f2.findField('MMCODE').getValue());
                            //T1Query.getForm().findField('P1').focus();
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Load();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
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
        viewport.down('#form').collapse();
        viewport.down('#t1Grid').unmask(); //左邊refresh結束
        var f = T1Form.getForm();

        //////////f.reset(); //在 setFormT1a 有設定
        ////////f.getFields().each(function (fc) { //右側欄位Read Only
        ////////    if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
        ////////        fc.setReadOnly(true);
        ////////    } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
        ////////        fc.readOnly = true;
        ////////    }
        ////////});

        f.findField('VISITKIND').setReadOnly(true);
        f.findField('LOCATION').setReadOnly(true);
        f.findField('ROUTINESTOCKCODE').setReadOnly(true);
        f.findField('BEGINTIME').setReadOnly(true);
        f.findField('ENDTIME').setReadOnly(true);
        f.findField('EXCEPTSTOCKCODE').setReadOnly(true);
        f.findField('TAKEOUTSTOCKCODE').setReadOnly(true);
        f.findField('RETURNSTOCKCODE').setReadOnly(true);

        f.findField('CHEMOSTOCKCODE').setReadOnly(true);
        f.findField('TPNSTOCKCODE').setReadOnly(true);
        f.findField('PCASTOCKCODE').setReadOnly(true);
        f.findField('DEFAULTSTOCKCODE').setReadOnly(true);
        f.findField('RESEARCHSTOCKCODE').setReadOnly(true);

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1RecLength === 0);        //新增完後T1RecLength是0，無法修改
        T1Grid.down('#delete').setDisabled(T1RecLength === 0);      //選擇左側T1Grid，一個會有T1LastRec，則可以修改

        //選擇左側T1Grid，一個會有T1LastRec，則可以修改
        if (T1LastRec) {
            viewport.down('#form').expand();
            isNew = false;
            T1Form.loadRecord(T1LastRec); //資料從T1Grid to T1Form
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('VISITKIND');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T1Form.getForm().reset();  //右側資料清空
        }
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

    viewport.down('#form').collapse();

});
