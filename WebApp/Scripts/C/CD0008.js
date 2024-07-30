Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var WhnoComboGet = '/api/CD0008/GetWhnoCombo';
    var DocnoComboPost = '/api/CD0008/GetDocnoCombo';
    var ChkWhGet = '/api/CD0008/GetChkWh';
    var MmcodeScanGet = '/api/CD0008/GetMmcodeScan';
    var T1Name = "裝箱出庫作業";

    var T1Rec = 0;
    var T1LastRec = null;

    var selected = [];
    var inputSource = '';

    var boxInfo = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var viewModel = Ext.create('WEBAPP.store.CD0008VM');

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME', 'WH_KIND', 'WH_USERID']
    });
    function getWhnoCombo() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        //whnoQueryStore.add({ WH_NO: '', WH_NAME: '', WH_KIND: '', WH_USERID:''});
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add({
                                WH_NO: wh_nos[i].WH_NO,
                                WH_NAME: wh_nos[i].WH_NAME,
                                WH_KIND: wh_nos[i].WH_KIND,
                                WH_USERID: wh_nos[i].WH_USERID,
                            });
                        }

                        T1Query.getForm().findField('P0').setValue(whnoQueryStore.data.items[0].data.WH_NO);

                        var f = T1Query.getForm();
                        if (f.findField('P0').getValue() &&
                            f.findField('P1').getValue() &&
                            f.findField('P2').getValue()) {
                            getDocnoCombo();
                        }
                    } else {
                        // 查不到您所屬庫房資料
                        Ext.Msg.alert('提醒', '查不到您所屬庫房資料');
                        return;
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getWhnoCombo();

    // 出庫狀態
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "N", "TEXT": "待出庫申請單" },
            { "VALUE": "Y", "TEXT": "已出庫申請單" }
        ]
    });

    // 申請單號清單
    var docnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function getDocnoCombo() {
        docnoStore.removeAll();
        T1Query.getForm().findField('P3').setValue('');
        T1Query.getForm().findField('P3').clearInvalid();
        Ext.Ajax.request({
            url: DocnoComboPost,
            method: reqVal_p,
            params: {
                P0: T1Query.getForm().findField('P0').getValue(),
                P1: T1Query.getForm().findField('P1').getValue(),
                P2: T1Query.getForm().findField('P2').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var docnos = data.etts;
                    if (docnos.length > 0) {
                        for (var i = 0; i < docnos.length; i++) {
                            docnoStore.add({
                                VALUE: docnos[i],
                                TEXT: docnos[i]
                            });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Store = viewModel.getStore('BC_WHPICK');
    function T1Load(clearMsg) {

        if (clearMsg) {
            msglabel('');
        }

        T1Store.getProxy().setExtraParam("P0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("P1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("P2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("P3", T1Query.getForm().findField('P3').getValue());
        T1Form.getForm().findField('T1DOCNO').setValue(T1Query.getForm().findField('P3').getValue());

        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true
    });

    var T1Query = Ext.widget({
        title: '查詢',
        itemId: 'queryform',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: true,
        hideCollapseTool: true,
        titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 35%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'combo',
                    store: whnoQueryStore,
                    name: 'P0',
                    id: 'P0',
                    fieldLabel: '庫房號碼',
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    queryMode: 'local',
                    fieldCls: 'required',
                    anyMatch: true,
                    allowBlank: false,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    padding: '0 4 0 4',
                    multiSelect: false,
                    //tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                    listeners: {
                        select: function (oldValue, newValue, eOpts) {
                            var f = T1Query.getForm();
                            if (f.findField('P0').getValue() &&
                                f.findField('P1').getValue() &&
                                f.findField('P2').getValue()) {
                                getDocnoCombo();
                            }
                        },
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) // 改為按整個field即展開下拉選項,以方便在手機畫面點picker
                            {
                                setTimeout(function () {
                                    field.expand(); // 若透過點picker下拉選項,會重複做expand,expand只需做一次就好   
                                }, 300);
                            }
                        }
                    }
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '揀貨日期',
                    name: 'P1',
                    id: 'P1',
                    fieldCls: 'required',
                    allowBlank: false,
                    value: new Date(),
                    padding: '0 4 0 4',
                    listeners: {
                        select: function () {
                            var f = T1Query.getForm();
                            if (f.findField('P0').getValue() &&
                                f.findField('P1').getValue() &&
                                f.findField('P2').getValue()) {
                                getDocnoCombo();
                            }
                        },
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) // 改為按整個field即展開下拉選項,以方便在手機畫面點picker
                            {
                                setTimeout(function () {
                                    field.expand(); // 若透過點picker下拉選項,會重複做expand,expand只需做一次就好   
                                }, 300);
                            }
                        }
                    }
                },
                {
                    xtype: 'combo',
                    store: statusStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '出庫狀態',
                    allowBlank: false,
                    name: 'P2',
                    id: 'P2',
                    fieldCls: 'required',
                    padding: '0 4 0 4',
                    //tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    enforceMaxLength: true,
                    listeners: {
                        select: function (self, newValue, eOpts) {

                            if (newValue.data.VALUE == "N") {
                                Ext.getCmp('putInBox').enable();
                                Ext.getCmp('putOutBox').enable();
                                Ext.getCmp('shipOut').enable();
                                Ext.getCmp('docShipOut').enable();
                                //Ext.getCmp('shipOutCancel').disable();
                            } else {
                                Ext.getCmp('putInBox').disable();
                                Ext.getCmp('putOutBox').disable();
                                Ext.getCmp('shipOut').disable();
                                Ext.getCmp('docShipOut').disable();
                                // Ext.getCmp('shipOutCancel').enable();
                            }
                            var f = T1Query.getForm();
                            if (f.findField('P0').getValue() &&
                                f.findField('P1').getValue() &&
                                f.findField('P2').getValue()) {
                                getDocnoCombo();
                            }
                        },
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) // 改為按整個field即展開下拉選項,以方便在手機畫面點picker
                            {
                                setTimeout(function () {
                                    field.expand(); // 若透過點picker下拉選項,會重複做expand,expand只需做一次就好   
                                }, 300);
                            }
                        }
                    }
                },
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'combo',
                    store: docnoStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '申請單號',
                    allowBlank: false,
                    fieldCls: 'required',
                    name: 'P3',
                    id: 'P3',
                    padding: '0 4 0 4',
                    //tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    enforceMaxLength: true,
                    listeners: {
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) // 改為按整個field即展開下拉選項,以方便在手機畫面點picker
                            {
                                setTimeout(function () {
                                    field.expand(); // 若透過點picker下拉選項,會重複做expand,expand只需做一次就好   
                                }, 300);
                            }
                        }
                    }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    name: 'P4',
                    id: 'P4',
                    listeners: {
                        change: function (field, nValue, oValue, eOpts) {
                            if (nValue.length >= 8) {
                                getMmcodeByScan(nValue);
                            }
                        }
                    }
                },
                {
                    xtype: 'panel',
                    border: false,
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'hbox',
                            }
                        },
                        'width >= 600': {
                            layout: {
                                type: 'hbox',
                                vertical: false
                            }
                        }
                    },
                    items: [{
                        xtype: 'button',
                        text: '查詢',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width:'50%'
                            },
                        },
                        margin: '1',
                        handler: function () {
                            var f = this.up('form').getForm();
                            if (!f.findField('P0').getValue() ||
                                !f.findField('P1').getValue() ||
                                !f.findField('P3').getValue())
                                Ext.Msg.alert('提醒', '庫房號碼、揀貨日期與申請單號為必填');
                            else {
                                Ext.ComponentQuery.query('panel[itemId=queryform]')[0].collapse();
                                T1Load(true);
                            }
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        plugins: 'responsive',
                        margin: '1',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            },
                            //'width >= 600': {

                            //}
                        },
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            //f.findField('P0').focus();
                        }
                    }]
                }, {
                    xtype: 'panel',
                    border: false,
                    id: 'queryExBtn',
                    hidden: true,
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'hbox',
                            }
                        },
                        'width >= 600': {
                            layout: {
                                type: 'hbox',
                                vertical: false
                            }
                        }
                    },
                    items: [{
                        xtype: 'button',
                        text: '分配揀貨',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '33%'
                            }
                        },
                        margin: '1',
                        handler: function () {
                            parent.link2('/Form/Index/CD0009', '藥庫分配揀貨人員(CD0009)', true);
                        }
                    }, {
                        xtype: 'button',
                        text: '揀貨',
                        plugins: 'responsive',
                        margin: '1',
                        responsiveConfig: {
                            'width < 600': {
                                width: '33%'
                            }
                        },
                        handler: function () {
                            parent.link2('/Form/Mobile/CD0004', ' 點選申請單編號(PDA)(CD0004)', true);
                        }
                    }, {
                        xtype: 'button',
                        text: '資料查詢',
                        plugins: 'responsive',
                        margin: '1',
                        responsiveConfig: {
                            'width < 600': {
                                width: '33%'
                            }
                        },
                        handler: function () {
                            parent.link2('/Form/Mobile/CD0010', '揀貨資料查詢(CD0010)', true);
                        }
                    }]
                }
            ]
        }]
    });

    var T1Form = Ext.widget({
        itemId: 't1Form',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 35%',
            width: '30%'
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelT1P1',
                border: false,
                layout: {
                    type: 'box',
                    vertical: true,
                    align: 'stretch'
                },
                items: [
                    {
                        xtype: 'panel',
                        border: false,
                        id: 'PanelT1P11',
                        layout: {
                            type: 'hbox',
                        },
                        items: [{
                            xtype: 'button',
                            name: 'putInBox',
                            id: 'putInBox',
                            text: '裝入物流箱',
                            margin: '1',
                            disabled: true,
                            plugins: 'responsive',
                            responsiveConfig: {
                                'width < 600': {
                                    width: '50%'
                                },
                                //'width >= 600': {

                                //}
                            },
                            handler: function () {
                                
                                if (boxInfo == null) {
                                    Ext.Msg.alert('提醒', '請輸入物流箱號碼或感應物流箱條碼');
                                    return;
                                }

                                if (T1Grid.getSelection().length == 0) {
                                    Ext.Msg.alert('提醒', '請點選要放入物流箱的品項');
                                    return;
                                }

                                var selection = T1Grid.getSelection();
                                var temp = [];
                                for (var i = 0; i < selection.length; i++) {
                                    
                                    selection[i].data.BOXNO = boxInfo.BOXNO;
                                    selection[i].data.BARCODE = boxInfo.BARCODE;
                                    selection[i].data.XCATEGORY = boxInfo.XCATEGORY;
                                    temp.push(selection[i].data);
                                }

                                putInBox(temp);
                            }
                        },
                        {
                            xtype: 'button',
                            text: '取消物流箱',
                            name: 'putOutBox',
                            id: 'putOutBox',
                            disabled: true,
                            margin: '1',
                            plugins: 'responsive',
                            responsiveConfig: {
                                'width < 600': {
                                    width: '50%'
                                },
                                //'width >= 600': {

                                //}
                            },
                            handler: function () {

                                //if (T1Grid.getSelection().length == 0) {
                                //    Ext.Msg.alert('提醒', '請點選要取消放入物流箱的品項');
                                //    return;
                                //}

                                var selection = T1Grid.getSelection();
                                var temp = [];
                                for (var i = 0; i < selection.length; i++) {
                                    
                                    if (selection[i].data.BOXNO != '') {
                                        temp.push(selection[i].data);
                                    }
                                }
                                if (temp.length == 0) {
                                    Ext.Msg.alert('提醒', '請點選要取消放入物流箱的品項');
                                    return; s
                                }

                                putOutBox(temp);
                            }
                        }]
                    },
                    {
                        xtype: 'panel',
                        id: 'PanelT1P12',
                        border: false,
                        plugins: 'responsive',
                        
                        responsiveConfig: {
                            'width < 600': {
                                layout: {
                                    type: 'hbox',
                                }
                            },
                            'width >= 600': {
                                layout: {
                                    type: 'hbox',
                                    vertical: false
                                }
                            }
                        },
                        items: [
                            {
                                xtype: 'button',
                                text: '出庫',
                                name: 'shipOut',
                                id: 'shipOut',
                                disabled: true,
                                plugins: 'responsive',
                                margin: '1',
                                responsiveConfig: {
                                    'width < 600': {
                                        width: '50%'
                                    },
                                },
                                handler: function () {
                                    if (T1Grid.getSelection().length == 0) {
                                        Ext.Msg.alert('提醒', '請點選要出庫的品項');
                                        return;
                                    }

                                    var selection = T1Grid.getSelection();
                                    var temp = [];
                                    for (var i = 0; i < selection.length; i++) {
                                        temp.push(selection[i].data);
                                    }

                                    Ext.MessageBox.confirm('出庫', '是否確定要進行出庫？', function (btn, text) {
                                        if (btn === 'yes') {
                                            shipOut(temp);
                                        }
                                    });
                                }
                            }, {
                                xtype: 'button',
                                text: '設定整個申請單出庫',
                                name: 'docShipOut',
                                id: 'docShipOut',
                                disabled: true,
                                plugins: 'responsive',
                                margin:'1',
                                responsiveConfig: {
                                    'width < 600': {
                                        width: '50%'
                                    },
                                    //'width >= 600': {

                                    //}
                                },
                                handler: function () {
                                    if (T1Form.getForm().findField('T1DOCNO').getValue() == null
                                        || T1Form.getForm().findField('T1DOCNO').getValue() == '')
                                        Ext.Msg.alert('訊息', '請先選擇申請單號!');
                                    else {
                                        if (T1Store.getCount() == 0)
                                            Ext.Msg.alert('訊息', '此申請單已無待出庫品項!');
                                        else
                                            chkBcWhpick();
                                    }
                                }
                            }
                            //, {
                            //    xtype: 'button',
                            //    text: '取消出庫',
                            //    name: 'shipOutCancel',
                            //    id: 'shipOutCancel',
                            //    disabled: true,
                            //    plugins: 'responsive',
                            //    margin:'1',
                            //    responsiveConfig: {
                            //        'width < 600': {
                            //            width: '50%'
                            //        },
                            //        //'width >= 600': {

                            //        //}
                            //    },
                            //    handler: function () {
                            //        if (T1Grid.getSelection().length == 0) {
                            //            Ext.Msg.alert('提醒', '請點選要取消出庫的品項');
                            //            return;
                            //        }

                            //        var selection = T1Grid.getSelection();
                            //        var temp = [];
                            //        for (var i = 0; i < selection.length; i++) {
                            //            temp.push(selection[i].data);
                            //        }

                            //        Ext.MessageBox.confirm('取消出庫', '是否確定要取消出庫？', function (btn, text) {
                            //            if (btn === 'yes') {
                            //                shipOutCancel(temp);
                            //            }
                            //        });
                            //    }
                            //},
                        ]
                    }
                ]
            },
            {
                xtype: 'panel',
                id: 'PanelT1P2',
                border: false,
                layout: {
                    type: 'box',
                    vertical: true,
                    align: 'stretch'
                },
                items: [
                    {
                        xtype: 'displayfield',
                        name: 'T1DOCNO',
                        id: 'T1DOCNO',
                        fieldLabel: '申請單號碼',
                        padding: '0 4 0 4',
                        labelStyle: 'width: 45%',
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '物流箱號碼',
                        name: 'T1P0',
                        id: 'T1P0',
                        enforceMaxLength: true,
                        maxLength: 20,
                        padding: '0 4 0 4',
                        labelStyle: 'width: 45%',
                        listeners: {
                            focus: function (field, event, eOpts) {
                                inputSource = 'boxNo';
                            }
                        }
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '條碼',
                        name: 'T1P1',
                        id: 'T1P1',
                        enforceMaxLength: true,
                        maxLength: 200,
                        padding: '0 4 0 4',
                        labelStyle: 'width: 25%',
                        listeners: {
                            focus: function (field, event, eOpts) {
                                inputSource = 'barcode';
                            }
                        }
                    }, {
                        xtype: 'button',
                        text: '查詢物流箱',
                        padding: '0 4 0 4',
                        margin:'1',
                        handler: function () {
                            var f = T1Form.getForm();
                            if (!f.findField('T1P0').getValue() && 
                                !f.findField('T1P1').getValue()) {
                                Ext.Msg.alert('提醒', '請輸入物流箱號碼或感應物流箱條碼');
                                return;
                            }
                            
                            var temp = T1Grid.getSelection();
                            var input = '';
                            if (inputSource == 'barcode') {
                                input = f.findField('T1P1').getValue();
                            } else if (inputSource == 'boxNo') {
                                input = f.findField('T1P0').getValue();
                            }

                            loadBoxInfo(input, inputSource);
                        }
                    },

                ]
            }
        ]
    });
    function putInBox(selection) {
        
        Ext.Ajax.request({
            url: '/api/CD0008/PutInBox',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(selection) },
            success: function (response) {
                
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    msglabel('裝入物流箱成功');
                    T1Load(false);

                } else {
                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }
    function putOutBox(selection) {
        
        Ext.Ajax.request({
            url: '/api/CD0008/PutOutBox',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(selection) },
            success: function (response) {
                
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    msglabel('取消物流箱成功');
                    T1Load(false);

                } else {
                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }
    function shipOut(selection) {
        
        Ext.Ajax.request({
            url: '/api/CD0008/ShipOut',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(selection) },
            success: function (response) {
                
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    msglabel('出貨成功');
                    T1Load(false);

                } else {
                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }
    function chkBcWhpick() {
        Ext.Ajax.request({
            url: '/api/CD0008/chkBcWhpick',
            method: reqVal_p,
            params: {
                wh_no: T1Query.getForm().findField('P0').getValue(),
                pick_date: T1Query.getForm().findField('P1').getValue(),
                docno: T1Form.getForm().findField('T1DOCNO').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    if (data.msg == '0')
                    {
                        Ext.MessageBox.confirm('訊息', '是否確定要將整個申請單待出庫品項均設定出庫?', function (btn, text) {
                            if (btn === 'yes') {
                                docShipOut();
                            }
                        });
                    }
                    else
                    {
                        Ext.MessageBox.confirm('訊息', '此申請單尚有' + data.msg + '項尚未揀貨，是否確定要設定全部出庫?', function (btn, text) {
                            if (btn === 'yes') {
                                docShipOut();
                            }
                        });
                    }
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }
    function docShipOut() {
        Ext.Ajax.request({
            url: '/api/CD0008/docShipOut',
            method: reqVal_p,
            params: {
                wh_no: T1Query.getForm().findField('P0').getValue(),
                pick_date: T1Query.getForm().findField('P1').getValue(),
                docno: T1Form.getForm().findField('T1DOCNO').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('整個申請單出庫完成');
                    T1Load(false);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }
    function shipOutCancel(selection) {
        
        Ext.Ajax.request({
            url: '/api/CD0008/ShipOutCancel',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(selection) },
            success: function (response) {
                
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    msglabel('取消出貨成功');
                    T1Load(false);

                } else {
                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }
    function loadBoxInfo(input, inputSource) {
        Ext.Ajax.request({
            url: '/api/CD0008/GetBcBox',
            method: reqVal_p,
            params: {
                input: input,
                source: inputSource
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    if (data.etts.length == 0) {
                        Ext.Msg.alert('提醒', '無此物流箱，請重新確認');

                        return;
                    }

                    boxInfo = data.etts[0];
                    if (boxInfo.STATUS == "N") {
                        Ext.Msg.alert('提醒', '此物流箱已停用，請不要使用此物流箱');
                        return;
                    }

                    T1Form.getForm().findField('T1P0').setValue(boxInfo.BOXNO);
                    T1Form.getForm().findField('T1P1').setValue(boxInfo.BARCODE);
                    
                }
            },
            failure: function (response, options) {

            }
        });
    }

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
            items: [T1Query]
        },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Form]
            },
            {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        selModel: {
            mode: 'SIMPLE'
        },
        selType: 'rowmodel',
        columns: {
            defaults: {
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        hidden: true
                    },
                    'width >= 600': {
                        hidden: false
                    }
                }
            },
            items: [
                {
                    text: "申請單號碼",
                    dataIndex: 'DOCNO',
                    width: 155
                },
                {
                    text: "申請部門",
                    dataIndex: 'APPDEPT',
                    //width: '30%'
                },
                {
                    text: "項次",
                    dataIndex: 'SEQ',
                    plugins: '',
                    width: '20%',
                    align: 'right',
                    style: 'text-align:left',
                },
                {
                    text: "院內碼",
                    dataIndex: 'MMCODE',
                    width: '35%',
                    plugins: '',
                },
                {
                    text: "中文品名",
                    dataIndex: 'MMNAME_C',
                },
                {
                    text: "英文品名",
                    dataIndex: 'MMNAME_E',
                },
                {
                    text: "申請數",
                    dataIndex: 'APPQTY',

                },
                {
                    text: "揀貨數",
                    dataIndex: 'ACT_PICK_QTY',
                },
                {
                    text: "撥補單位",
                    dataIndex: 'BASE_UNIT',
                },
                {
                    text: "物流箱號",
                    dataIndex: 'BOXNO',
                    plugins: '',
                    width: '30%',
                }, {
                    text: " ",
                    align: 'left',
                    flex: 1,
                    responsiveConfig: {
                        'width < 600': {
                            hidden: false
                        },
                        'width >= 600': {
                            hidden: true
                        }
                    },
                    renderer: function (val, meta, record) {
                        return '<a href=\'javascript:popItemDetail();\'><img src="../../../Images/TRA/TRASearch.gif" width="20vh" height="20vh" align="absmiddle" /></a>';
                    }
                }],
        },
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                
                T1LastRec = record;
            },
        }
        
    });

    var callableWin = null;
    popItemDetail = function () {
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'itemDetail',
                height: '95%',
                layout: 'fit',
                closable: false,
                border: true,
                fieldDefaults: {
                    labelAlign: 'right',
                    labelWidth: 90,
                    width: '100%'
                },
                items: [
                    {
                        xtype: 'container',
                        layout: 'vbox',
                        padding: '2vmin',
                        scrollable: true,
                        items: [
                            {
                                xtype: 'displayfield',
                                fieldLabel: '申請單號碼',
                                name: 'DOCNO',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '申請部門',
                                name: 'APPDEPT',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '項次',
                                name: 'SEQ',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '院內碼',
                                name: 'MMCODE',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '中文品名',
                                name: 'MMNAME_C',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '英文品名',
                                name: 'MMNAME_E',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '申請數',
                                name: 'APPQTY',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '揀貨數',
                                name: 'ACT_PICK_QTY',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '撥補單位',
                                name: 'BASE_UNIT',
                                readOnly: true
                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '物流箱號',
                                name: 'BOXNO',
                                readOnly: true
                            }
                        ]
                    }
                ],
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '<font size="3vmin">關閉</font>',
                    height: '6vmin',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });

            callableWin = GetPopWin(viewport, popform, '品項資料明細', viewport.width * 0.9, viewport.height * 0.7);
        }
        callableWin.show();
        popform.loadRecord(T1LastRec);
    }

    function getMmcodeByScan(parMmcode) {
        Ext.Ajax.request({
            url: MmcodeScanGet,
            method: reqVal_p,
            params: {
                p0: parMmcode
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var getMmcode = data.msg;
                    var selection = T1Store.findRecord('MMCODE', getMmcode);
                    if (selection != null) {
                        var temp = [];
                        temp.push(selection.data);
                        shipOut(temp);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function chkWh() {
        Ext.Ajax.request({
            url: ChkWhGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var isPh = data.msg;
                    if (isPh == 'Y')
                        Ext.getCmp('queryExBtn').setVisible(true);
                    else
                        Ext.getCmp('queryExBtn').setVisible(false);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    chkWh();

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
        }]
    });

    //T1Query.getForm().findField('P0').focus();
    //T1Query.getForm().findField('P0').setValue(session['Inid']);
    T1Query.getForm().findField('P2').setValue('N');

    Ext.getCmp('putInBox').enable();
    Ext.getCmp('putOutBox').enable();
    Ext.getCmp('shipOut').enable();
    Ext.getCmp('docShipOut').enable();

    Ext.getDoc().dom.title = T1Name;

});
